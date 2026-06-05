window.initFooterAutoHide = function () {
    const footer = document.getElementById('appFooter');
    if (!footer) return;

    const HIDE_DELAY_MS = 3000;
    let timerId = null;

    function showFooter() { footer.classList.remove('footer-hidden'); }
    function hideFooter() { footer.classList.add('footer-hidden'); }
    function resetTimer() {
        showFooter();
        if (timerId) window.clearTimeout(timerId);
        timerId = window.setTimeout(hideFooter, HIDE_DELAY_MS);
    }

    resetTimer();

    const events = ['mousemove', 'wheel', 'keydown', 'touchstart', 'pointermove'];
    events.forEach(function (ev) {
        window.addEventListener(ev, resetTimer, { passive: true });
    });

    footer.addEventListener('mouseenter', function () {
        if (timerId) window.clearTimeout(timerId);
        showFooter();
    });
    footer.addEventListener('mouseleave', resetTimer);

    window.addEventListener('beforeunload', function () {
        if (timerId) window.clearTimeout(timerId);
        events.forEach(function (ev) {
            window.removeEventListener(ev, resetTimer);
        });
    });
};
