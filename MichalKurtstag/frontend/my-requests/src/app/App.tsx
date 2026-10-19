import { useMyRequests } from './useMyRequests';

function formatDate(date: string): string {
  return new Intl.DateTimeFormat('he-IL', { day: 'numeric', month: 'long', year: 'numeric' }).format(new Date(date));
}

export default function App() {
  const { requests, loading, error, retry } = useMyRequests();
  const sortedRequests = [...requests].sort((a, b) =>
    Number(b.actionRequired) - Number(a.actionRequired)
    || Date.parse(b.lastUpdate) - Date.parse(a.lastUpdate)
  );

  return (
    <main className="page" dir="rtl">

      <section id="main" className="content" aria-labelledby="page-title">
        <div className="heading-row">
          <div>
            <p className="eyebrow"><span className="eyebrow-mark" aria-hidden="true" />האזור האישי <span className="breadcrumb-divider" aria-hidden="true">/</span> השירותים שלי</p>
            <h1 id="page-title">הבקשות שלי<span className="title-period" aria-hidden="true">.</span></h1>
            <p className="intro">כל העדכונים מהרשויות, מרוכזים כאן.</p>
          </div>
          {!loading && !error && requests.length > 0 && (
            <div className="count" aria-live="polite"><strong>{requests.length.toString().padStart(2, '0')}</strong><span>בקשות<br />במעקב</span></div>
          )}
        </div>

        {loading ? (
          <div className="state-panel" role="status" aria-live="polite">
            <span className="loader" aria-hidden="true" /><span>טוענים את הבקשות שלך...</span>
          </div>
        ) : error ? (
          <div className="error-panel" role="alert">
            <span className="state-icon" aria-hidden="true">!</span>
            <div><h2>הבקשות לא זמינות כרגע</h2><p>{error}</p></div>
            <button className="retry-button" type="button" onClick={retry} disabled={loading}>
              <span aria-hidden="true">↻</span> לנסות שוב
            </button>
          </div>
        ) : requests.length === 0 ? (
          <div className="empty-state" role="status">
            <div className="empty-illustration" aria-hidden="true"><span>✓</span></div>
            <h2>עדיין לא הגשת בקשות</h2>
            <p>בקשות לשירותים עירוניים והעדכונים שלהן יופיעו כאן.</p>
          </div>
        ) : (
          <>
            <div className="list-header" aria-hidden="true"><span>פרטי הבקשה</span><span>עדכון אחרון</span></div>
            <ul className="request-list" aria-label="רשימת הבקשות שלי">
              {sortedRequests.map(request => (
                <li key={request.requestId} className={`request-row${request.actionRequired ? ' needs-action' : ''}${request.isOverdue ? ' overdue' : ''}`}>
                  <div className="request-main">
                    <div className="request-title-row">
                      <h2>{request.serviceName}</h2>
                      {request.isOverdue && <span className="overdue-tag"><span aria-hidden="true">◷</span> חרג מזמן הטיפול</span>}
                    </div>
                    <p className="authority">{request.authorityName}</p>
                    <div className="status-line">
                      {request.actionRequired && <span className="action-dot" aria-hidden="true" />}
                      <span className={`status${request.actionRequired ? ' action-status' : ''}`}>
                        {request.actionRequired ? 'נדרשת פעולה: ' : ''}{request.status}
                      </span>
                    </div>
                    {request.actionRequired && (
                      <a className="action-link" href={`/personal-area/requests/${encodeURIComponent(request.requestId)}`} aria-label={`השלמת פעולה עבור ${request.serviceName}`}>
                        להשלמת הפעולה <span aria-hidden="true">←</span>
                      </a>
                    )}
                  </div>
                  <div className="request-meta">
                    <span className="mobile-label">עדכון אחרון</span>
                    <time dateTime={request.lastUpdate}>{formatDate(request.lastUpdate)}</time>
                    <span className="request-id">מספר בקשה {request.requestId}</span>
                  </div>
                </li>
              ))}
            </ul>
          </>
        )}
      </section>
    </main>
  );
}