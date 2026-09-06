import { Component, EventEmitter, Input, Output, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ChatApi, ChatMessage } from '../../services/chat-api';
import { Spinner } from '../spinner/spinner';
import { ChatMarkdownPipe } from './chat-markdown.pipe';

@Component({
  selector: 'app-chat',
  imports: [FormsModule, Spinner, ChatMarkdownPipe],
  templateUrl: './chat.html',
  styleUrl: './chat.css',
})
export class Chat {
  @Input() expanded = false;
  @Output() closed = new EventEmitter<void>();
  @Output() expandedChange = new EventEmitter<boolean>();

  private readonly api = inject(ChatApi);
  readonly messages = signal<ChatMessage[]>([]);
  readonly draft = signal('');
  readonly sending = signal(false);
  readonly error = signal('');
  readonly conversationId = signal<string | null>(localStorage.getItem('calory_conversation_id'));
  readonly prompts = [
    'How am I tracking today?',
    'What should I focus on this week?',
    'Summarize my recent meals.',
  ];

  get draftValue(): string {
    return this.draft();
  }
  set draftValue(value: string) {
    this.draft.set(value);
  }

  close(): void {
    this.closed.emit();
  }
  toggleExpanded(): void {
    this.expandedChange.emit(!this.expanded);
  }

  async send(text = this.draft()): Promise<void> {
    const message = text.trim();
    if (!message || this.sending()) return;
    this.draft.set('');
    this.error.set('');
    this.sending.set(true);
    this.messages.update((items) => [
      ...items,
      { role: 'user', text: message },
      { role: 'assistant', text: '' },
    ]);
    try {
      const id = await this.api.stream(message, this.conversationId(), (chunk) =>
        this.messages.update((items) => {
          const next = [...items];
          const last = next.length - 1;
          next[last] = { ...next[last], text: next[last].text + chunk };
          return next;
        }),
      );
      this.conversationId.set(id);
      localStorage.setItem('calory_conversation_id', id);
    } catch (error) {
      this.error.set(error instanceof Error ? error.message : 'The assistant is unavailable.');
      this.messages.update((items) => items.slice(0, -1));
    } finally {
      this.sending.set(false);
    }
  }
}
