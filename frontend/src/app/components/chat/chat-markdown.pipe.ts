import { Pipe, PipeTransform, SecurityContext, inject } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';

@Pipe({
  name: 'chatMarkdown',
  standalone: true,
})
export class ChatMarkdownPipe implements PipeTransform {
  private readonly sanitizer = inject(DomSanitizer);

  transform(value: string): string {
    const escaped = this.escapeHtml(value);
    const lines = escaped.split(/\r?\n/);
    const output: string[] = [];
    let inList = false;

    for (const line of lines) {
      const bullet = line.match(/^\s*-\s+(.*)$/);
      if (bullet) {
        if (!inList) {
          output.push('<ul>');
          inList = true;
        }
        output.push(`<li>${this.formatInline(bullet[1])}</li>`);
        continue;
      }

      if (inList) {
        output.push('</ul>');
        inList = false;
      }

      if (line.trim()) output.push(`<p>${this.formatInline(line)}</p>`);
    }

    if (inList) output.push('</ul>');
    return this.sanitizer.sanitize(SecurityContext.HTML, output.join('')) ?? '';
  }

  private formatInline(value: string): string {
    return value.replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>');
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#039;');
  }
}