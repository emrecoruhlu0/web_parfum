// Like toggle — AJAX, sayfa yenilenmeden buton ve sayıyı günceller
document.addEventListener('click', async (e) => {
    const btn = e.target.closest('[data-like-toggle]');
    if (!btn) return;
    e.preventDefault();

    const perfumeId = btn.dataset.likeToggle;
    const form = btn.closest('form');
    const token = form?.querySelector('input[name="__RequestVerificationToken"]')?.value;
    if (!token) return;

    btn.disabled = true;
    try {
        const res = await fetch('/Likes/Toggle', {
            method: 'POST',
            headers: {
                'Accept': 'application/json',
                'X-Requested-With': 'fetch',
                'Content-Type': 'application/x-www-form-urlencoded',
            },
            body: `perfumeId=${encodeURIComponent(perfumeId)}&__RequestVerificationToken=${encodeURIComponent(token)}`,
        });
        if (!res.ok) {
            if (res.status === 401) { window.location = '/Auth/Login'; return; }
            throw new Error('HTTP ' + res.status);
        }
        const data = await res.json();

        // Buton görünümü
        btn.classList.toggle('liked', data.liked);
        const icon = btn.querySelector('[data-like-icon]');
        if (icon) icon.textContent = data.liked ? '♥' : '♡';
        const label = btn.querySelector('[data-like-label]');
        if (label) label.textContent = data.liked ? 'Beğenildi' : 'Beğen';

        // Sayı
        const countEl = document.querySelector(`[data-like-count="${perfumeId}"]`);
        if (countEl) countEl.textContent = data.count;
    } catch (err) {
        console.error('Like toggle failed:', err);
    } finally {
        btn.disabled = false;
    }
});
