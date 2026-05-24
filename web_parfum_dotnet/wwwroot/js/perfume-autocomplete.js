// Parfüm autocomplete — herhangi bir input'a data-perfume-autocomplete eklenince
// /Perfumes/Search endpoint'inden öneri çeker ve seçileni gizli inputa yazar.
//
// Markup:
//   <div class="perfume-autocomplete">
//     <input type="text" class="form-control" data-perfume-autocomplete
//            data-target="perfumeId" placeholder="Parfüm ara..." />
//     <input type="hidden" name="perfumeId" id="perfumeId" />
//     <div class="perfume-autocomplete-results"></div>
//   </div>

(function () {
    const PLACEHOLDER = 'https://placehold.co/36x36/f8e8d0/d97706?text=%3F';

    function debounce(fn, ms) {
        let t;
        return function (...args) {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, args), ms);
        };
    }

    function renderResults(container, items, onPick) {
        container.innerHTML = '';
        if (!items || items.length === 0) {
            container.innerHTML = '<div class="perfume-autocomplete-empty">Sonuç bulunamadı</div>';
            container.classList.add('show');
            return;
        }
        items.forEach((p, idx) => {
            const row = document.createElement('div');
            row.className = 'perfume-autocomplete-item' + (idx === 0 ? ' active' : '');
            row.dataset.id = p.id;
            row.dataset.name = p.name;
            row.dataset.brand = p.brand || '';
            row.dataset.image = p.imageUrl || '';
            const img = p.imageUrl || PLACEHOLDER;
            row.innerHTML = `
                <img src="${img}" alt="" onerror="this.src='${PLACEHOLDER}'" />
                <div class="pa-meta">
                    <div class="pa-name"></div>
                    <div class="pa-brand"></div>
                </div>`;
            row.querySelector('.pa-name').textContent = p.name;
            row.querySelector('.pa-brand').textContent = p.brand || '';
            row.addEventListener('mousedown', (e) => {
                e.preventDefault();
                onPick(p);
            });
            container.appendChild(row);
        });
        container.classList.add('show');
    }

    function setupAutocomplete(input) {
        if (input.dataset.acInit === '1') return;
        input.dataset.acInit = '1';

        const wrap = input.closest('.perfume-autocomplete');
        if (!wrap) return;
        const results = wrap.querySelector('.perfume-autocomplete-results');
        const targetName = input.dataset.target;
        const hidden = wrap.querySelector(`input[type="hidden"][name="${targetName}"]`)
            || document.getElementById(targetName);
        const chipHolder = wrap.querySelector('.perfume-chip-holder');

        function clearSelection() {
            if (hidden) hidden.value = '';
            if (chipHolder) chipHolder.innerHTML = '';
        }

        function showChip(p) {
            if (!chipHolder) return;
            const img = p.imageUrl || PLACEHOLDER;
            const chip = document.createElement('span');
            chip.className = 'perfume-chip-selected';
            chip.innerHTML = `
                <img src="${img}" alt="" onerror="this.src='${PLACEHOLDER}'" />
                <span class="pc-text"></span>
                <button type="button" class="pc-clear" aria-label="Kaldır">&times;</button>`;
            chip.querySelector('.pc-text').textContent = `${p.name} — ${p.brand || ''}`;
            chip.querySelector('.pc-clear').addEventListener('click', () => {
                clearSelection();
                input.value = '';
                input.style.display = '';
                chipHolder.innerHTML = '';
                input.focus();
            });
            chipHolder.innerHTML = '';
            chipHolder.appendChild(chip);
        }

        const fetchResults = debounce(async (q) => {
            if (!q || q.trim().length < 1) {
                results.classList.remove('show');
                return;
            }
            try {
                const res = await fetch(`/Perfumes/Search?q=${encodeURIComponent(q)}&limit=10`, {
                    headers: { 'Accept': 'application/json' },
                });
                if (!res.ok) return;
                const data = await res.json();
                renderResults(results, data, (p) => {
                    if (hidden) hidden.value = p.id;
                    input.value = `${p.name} — ${p.brand || ''}`;
                    results.classList.remove('show');
                    if (chipHolder) {
                        input.value = '';
                        showChip(p);
                    }
                });
            } catch (err) {
                console.error('Autocomplete failed:', err);
            }
        }, 200);

        input.addEventListener('input', () => {
            if (hidden && hidden.value) hidden.value = '';
            fetchResults(input.value);
        });

        input.addEventListener('focus', () => {
            if (input.value.trim().length >= 1) fetchResults(input.value);
        });

        input.addEventListener('blur', () => {
            setTimeout(() => results.classList.remove('show'), 150);
        });

        // Klavye navigasyonu
        input.addEventListener('keydown', (e) => {
            const items = Array.from(results.querySelectorAll('.perfume-autocomplete-item'));
            if (items.length === 0) return;
            let idx = items.findIndex(i => i.classList.contains('active'));
            if (e.key === 'ArrowDown') {
                e.preventDefault();
                if (idx < items.length - 1) idx++;
                items.forEach(i => i.classList.remove('active'));
                items[idx].classList.add('active');
                items[idx].scrollIntoView({ block: 'nearest' });
            } else if (e.key === 'ArrowUp') {
                e.preventDefault();
                if (idx > 0) idx--;
                items.forEach(i => i.classList.remove('active'));
                items[idx].classList.add('active');
                items[idx].scrollIntoView({ block: 'nearest' });
            } else if (e.key === 'Enter') {
                if (idx >= 0) {
                    e.preventDefault();
                    items[idx].dispatchEvent(new MouseEvent('mousedown'));
                }
            } else if (e.key === 'Escape') {
                results.classList.remove('show');
            }
        });
    }

    function init() {
        document.querySelectorAll('input[data-perfume-autocomplete]').forEach(setupAutocomplete);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    // Modal/dinamik içerikler için global hook
    window.initPerfumeAutocomplete = init;
})();
