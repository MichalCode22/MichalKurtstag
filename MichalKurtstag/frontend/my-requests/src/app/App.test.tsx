import { cleanup, fireEvent, render, screen, waitFor } from '@testing-library/react';
import '@testing-library/jest-dom/vitest';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import App from './App';
import { getMyRequests, type MyRequestsResponse } from './requestsApi';

vi.mock('./requestsApi', () => ({
  getMyRequests: vi.fn(),
  getRequestErrorMessage: vi.fn(() => 'לא הצלחנו לטעון את הבקשות כרגע.')
}));

const requestResponse: MyRequestsResponse = {
  requests: [{
    requestId: 'R-1',
    serviceName: 'דיווח על מפגע',
    authorityName: 'עיריית חדרה',
    status: 'נדרשים מסמכים',
    actionRequired: true,
    isOverdue: true,
    submittedAt: '2026-10-01T00:00:00Z',
    lastUpdate: '2026-10-02T00:00:00Z'
  }]
};

describe('My Requests UI', () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    vi.resetAllMocks();
  });

  it('announces loading while requests are being fetched', () => {
    vi.mocked(getMyRequests).mockReturnValue(new Promise(() => {}));

    render(<App />);

    expect(screen.getByRole('status')).toHaveTextContent('טוענים את הבקשות שלך');
  });

  it('shows an empty state when there are no requests', async () => {
    vi.mocked(getMyRequests).mockResolvedValue({ requests: [] });

    render(<App />);

    expect(await screen.findByText('עדיין לא הגשת בקשות')).toBeInTheDocument();
  });

  it('shows a friendly error and retries the request', async () => {
    vi.mocked(getMyRequests)
      .mockRejectedValueOnce(new Error('unavailable'))
      .mockResolvedValueOnce(requestResponse);

    render(<App />);

    expect(await screen.findByRole('alert')).toHaveTextContent('לא הצלחנו לטעון את הבקשות כרגע.');
    fireEvent.click(screen.getByRole('button', { name: /לנסות שוב/ }));

    expect(await screen.findByRole('link', { name: 'השלמת פעולה עבור דיווח על מפגע' })).toBeInTheDocument();
    expect(getMyRequests).toHaveBeenCalledTimes(2);
  });

  it('renders requests in RTL with action, overdue and semantic list indicators', async () => {
    vi.mocked(getMyRequests).mockResolvedValue(requestResponse);

    render(<App />);

    const list = await screen.findByRole('list', { name: 'רשימת הבקשות שלי' });
    expect(document.querySelector('main')).toHaveAttribute('dir', 'rtl');
    expect(list).toContainElement(screen.getByText('נדרשת פעולה: נדרשים מסמכים'));
    expect(screen.getByText(/חרג מזמן הטיפול/)).toBeInTheDocument();
    expect(screen.getByRole('link', { name: 'השלמת פעולה עבור דיווח על מפגע' })).toBeInTheDocument();
  });

  it('makes the action link keyboard-focusable', async () => {
    vi.mocked(getMyRequests).mockResolvedValue(requestResponse);

    render(<App />);

    const actionLink = await screen.findByRole('link', { name: 'השלמת פעולה עבור דיווח על מפגע' });
    actionLink.focus();
    await waitFor(() => expect(actionLink).toHaveFocus());
  });
});