import { envConfig as config } from './config';

interface MailHogMessage {
  Content: {
    Headers: {
      To?: string[];
      Subject?: string[];
    };
    Body: string;
  };
}

interface MailHogResponse {
  items: MailHogMessage[];
}


function extractToken(body: string): string | null {
  // 1. Extract everything after the phrase
  const match = body.match(/use this token: ([\s\S]+)/i);
  if (!match) return null;

  let tokenBlock = match[1];

  // 2. Remove quoted-printable soft line breaks (="\r\n" or ="\n")
  tokenBlock = tokenBlock.replace(/=\r?\n/g, '');

  // 3. Remove all whitespace (including newlines)
  let token = tokenBlock.replace(/\s/g, '');

  // 4. Trim (shouldn't be needed, but for safety)
  return token.trim();
}

export async function purgeMailhog(): Promise<void> {
  const res = await fetch(config.mailhogUrl + 'api/v1/messages', { method: 'DELETE' });
  if (!res.ok) {
    throw new Error(`Failed to purge MailHog messages: ${res.status} ${res.statusText}`);
  }
}

export async function getTokenFromMailhog(
  recipient: string,
  subjectContains: string
): Promise<string | null> {
  const response = await fetch(config.mailhogUrl + 'api/v2/messages');
  if (!response.ok) {
    throw new Error(`Failed to fetch MailHog messages: ${response.status} ${response.statusText}`);
  }
  const data = (await response.json()) as MailHogResponse;

  for (const item of data.items) {
    const to = item.Content.Headers.To?.[0] || '';
    const subject = item.Content.Headers.Subject?.[0] || '';
    const body = item.Content.Body || '';

    if (to.includes(recipient) && subject.includes(subjectContains)) {
      // Use the improved extraction logic
      const token = extractToken(body);
      if (token) {
        return token.replace(/\s+/g, '').trim();
      }
    }
  }
  return null;
}