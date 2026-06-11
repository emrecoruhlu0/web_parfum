(function () {
    const sb = document.getElementById('sidebarNav');
    const bd = document.getElementById('sidebarBackdrop');
    const btn = document.getElementById('sidebarToggle');
    if (!sb || !bd || !btn) return;

    function open() {
        sb.classList.add('show');
        bd.classList.add('show');
    }

    function close() {
        sb.classList.remove('show');
        bd.classList.remove('show');
    }

    btn.addEventListener('click', function () {
        sb.classList.contains('show') ? close() : open();
    });
    bd.addEventListener('click', close);
    sb.querySelectorAll('a.sidebar-link, a.nav-link-koku').forEach(function (a) {
        a.addEventListener('click', close);
    });
})();
