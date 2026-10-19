import { useEffect, useState } from 'react';
import { getMyRequests, getRequestErrorMessage, type MyRequest } from './requestsApi';

export function useMyRequests() {
  const [requests, setRequests] = useState<MyRequest[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  async function loadRequests(signal?: AbortSignal) {
    setLoading(true);
    setError('');
    try {
      const response = await getMyRequests(signal);
      setRequests(response.requests);
    } catch (reason) {
      if (!signal?.aborted) setError(getRequestErrorMessage(reason));
    } finally {
      if (!signal?.aborted) setLoading(false);
    }
  }

  useEffect(() => {
    const controller = new AbortController();
    void loadRequests(controller.signal);
    return () => controller.abort();
  }, []);

  return { requests, loading, error, retry: () => void loadRequests() };
}