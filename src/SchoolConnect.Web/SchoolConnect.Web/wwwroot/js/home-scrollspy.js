window.schoolConnectScrollSpy = (function () {
    let scrollHandler = null;
    let resizeHandler = null;
    let hashChangeHandler = null;
    let mutationObserver = null;
    let syncTimeout = null;
    let activeLinkSelector = '[data-scrollspy-link]';

    function getIsHomePage() {
        return window.location.pathname === '/';
    }

    function getTargets() {
        return Array.from(document.querySelectorAll('[data-scrollspy-target]'))
            .map((element) => ({
                element,
                target: element.getAttribute('data-scrollspy-target'),
            }))
            .filter((item) => item.target);
    }

    function getLinks() {
        return Array.from(document.querySelectorAll(activeLinkSelector))
            .reduce((map, link) => {
                const target = link.getAttribute('data-scrollspy-link');
                if (target) {
                    map.set(target, link);
                }
                return map;
            }, new Map());
    }

    function clearActive() {
        document.querySelectorAll(activeLinkSelector).forEach((link) => {
            link.classList.remove('active');
            link.removeAttribute('aria-current');
        });
    }

    function setActive(target, links) {
        clearActive();

        const link = links.get(target);
        if (!link) {
            return;
        }

        link.classList.add('active');
        link.setAttribute('aria-current', 'page');
    }

    function pickCurrentTarget(targets) {
        const scrollY = window.scrollY || window.pageYOffset || 0;
        const viewportMarker = scrollY + (window.innerHeight * 0.28);
        let current = targets[0] ?? null;

        for (const item of targets) {
            const top = item.element.getBoundingClientRect().top + scrollY;
            if (top <= viewportMarker) {
                current = item;
            }
        }

        const pageBottom = scrollY + window.innerHeight;
        const docHeight = Math.max(document.body.scrollHeight, document.documentElement.scrollHeight);
        if (docHeight - pageBottom < 240) {
            const contacts = targets.find((item) => item.target === 'contacts');
            if (contacts) {
                current = contacts;
            }
        }

        return current;
    }

    function sync() {
        if (!getIsHomePage()) {
            clearActive();
            destroyListeners();
            return;
        }

        const update = () => {
            const targets = getTargets();
            const links = getLinks();

            if (!targets.length || !links.size) {
                clearActive();
                return;
            }

            const current = pickCurrentTarget(targets);
            if (current) {
                setActive(current.target, links);
            }
        };

        destroyListeners();
        scrollHandler = () => update();
        resizeHandler = () => update();
        hashChangeHandler = () => window.setTimeout(update, 80);
        window.addEventListener('scroll', scrollHandler, { passive: true });
        window.addEventListener('resize', resizeHandler);
        window.addEventListener('hashchange', hashChangeHandler);
        update();
    }

    function scheduleSync() {
        window.clearTimeout(syncTimeout);
        syncTimeout = window.setTimeout(sync, 50);
    }

    function start() {
        scheduleSync();

        if (!mutationObserver) {
            mutationObserver = new MutationObserver(scheduleSync);
            mutationObserver.observe(document.body, {
                childList: true,
                subtree: true,
            });
        }
    }

    function destroyListeners() {
        if (scrollHandler) {
            window.removeEventListener('scroll', scrollHandler);
            scrollHandler = null;
        }

        if (resizeHandler) {
            window.removeEventListener('resize', resizeHandler);
            resizeHandler = null;
        }

        if (hashChangeHandler) {
            window.removeEventListener('hashchange', hashChangeHandler);
            hashChangeHandler = null;
        }
    }

    return {
        sync,
        start,
    };
})();

if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', window.schoolConnectScrollSpy.start, { once: true });
} else {
    window.schoolConnectScrollSpy.start();
}

window.addEventListener('pageshow', window.schoolConnectScrollSpy.start);
document.addEventListener('enhancedload', window.schoolConnectScrollSpy.start);
