(function () {
    function initStarRating(wrap) {
        if (!wrap || wrap.dataset.starInit === '1') return;
        wrap.dataset.starInit = '1';
        var inputId = wrap.dataset.ratingInput;
        var input = inputId ? document.getElementById(inputId) : wrap.querySelector('input[type="hidden"]');
        if (!input) return;
        var stars = wrap.querySelectorAll('i[data-value]');
        function paint(val) {
            stars.forEach(function (s) {
                var v = parseInt(s.dataset.value, 10);
                s.classList.toggle('bi-star-fill', v <= val);
                s.classList.toggle('bi-star', v > val);
            });
        }
        stars.forEach(function (s) {
            s.addEventListener('click', function () {
                var v = parseInt(s.dataset.value, 10);
                input.value = v;
                paint(v);
            });
            s.addEventListener('mouseenter', function () {
                paint(parseInt(s.dataset.value, 10));
            });
        });
        wrap.addEventListener('mouseleave', function () {
            paint(parseInt(input.value, 10) || 0);
        });
    }

    document.querySelectorAll('[data-star-rating]').forEach(initStarRating);

    window.initStarRating = function (root) {
        (root || document).querySelectorAll('[data-star-rating]').forEach(initStarRating);
    };
})();
