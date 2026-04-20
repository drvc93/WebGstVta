/**
 * Mensaje legible desde respuestas de error de HttpClient / API.
 */
export function readApiError(err: unknown, fallback: string): string {
  if (!err || typeof err !== 'object') return fallback;
  const e = err as { error?: unknown; message?: string };
  const body = e.error;
  if (typeof body === 'string' && body.trim()) return body;
  if (body && typeof body === 'object') {
    const d = (body as { detail?: unknown; title?: unknown }).detail ?? (body as { title?: unknown }).title;
    if (typeof d === 'string' && d.trim()) return d;
  }
  if (typeof e.message === 'string' && e.message.trim()) return e.message;
  return fallback;
}
