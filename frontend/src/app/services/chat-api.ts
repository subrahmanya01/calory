import { Injectable } from '@angular/core';
import { environment } from '../../environments/environment';

export interface ChatMessage { role: 'user' | 'assistant'; text: string; }

@Injectable({ providedIn: 'root' })
export class ChatApi {
  async stream(message: string, conversationId: string | null, onText: (text: string) => void): Promise<string> {
    const id = conversationId ?? crypto.randomUUID();
    const response = await fetch(`${environment.AiOrchestratorUrl}/chat/stream`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${localStorage.getItem('calory_token') ?? ''}` },
      body: JSON.stringify({ message, conversationId: id }),
    });
    if (!response.ok || !response.body) throw new Error('The assistant is unavailable.');
    const reader = response.body.getReader(); const decoder = new TextDecoder(); let buffer = '';
    while (true) {
      const chunk = await reader.read(); if (chunk.done) break;
      buffer += decoder.decode(chunk.value, { stream: true }); const events = buffer.split('\n\n'); buffer = events.pop() ?? '';
      for (const event of events) { const line = event.split('\n').find((value) => value.startsWith('data: ')); if (!line) continue; const data = line.slice(6); if (data === '[DONE]') continue; try { onText(JSON.parse(data) as string); } catch { onText(data); } }
    }
    return id;
  }
}