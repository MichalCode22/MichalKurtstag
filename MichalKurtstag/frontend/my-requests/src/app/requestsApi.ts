export interface MyRequest {
  requestId: string;
  serviceName: string;
  authorityName: string;
  status: string;
  actionRequired: boolean;
  isOverdue: boolean;
  submittedAt: string;
  lastUpdate: string;
}

export interface MyRequestsResponse {
  requests: MyRequest[];
}

async function getAccessToken(): Promise<string | null> {
  const existingToken = sessionStorage.getItem('access_token');
  if (existingToken) return existingToken;
  if (!import.meta.env.DEV) return null;

  const response = await fetch('/api/dev-auth/token');
  if (!response.ok) throw new Error('unavailable');

  const result = await response.json() as { accessToken: string };
  sessionStorage.setItem('access_token', result.accessToken);
  return result.accessToken;
}

export async function getMyRequests(signal?: AbortSignal): Promise<MyRequestsResponse> {
  const token = await getAccessToken();
  const response = await fetch('/api/my-requests', {
    signal,
    headers: token ? { Authorization: `Bearer ${token}` } : undefined
  });

  if (!response.ok) {
    throw new Error(response.status === 401 ? 'unauthorized' : 'unavailable');
  }

  return response.json() as Promise<MyRequestsResponse>;
}

export function getRequestErrorMessage(error: unknown): string {
  if (error instanceof Error && error.message === 'unauthorized') {
    return 'פג תוקף החיבור לאזור האישי. יש להתחבר מחדש ולנסות שוב.';
  }
  return 'לא הצלחנו לטעון את הבקשות כרגע. אפשר לנסות שוב בעוד רגע.';
}