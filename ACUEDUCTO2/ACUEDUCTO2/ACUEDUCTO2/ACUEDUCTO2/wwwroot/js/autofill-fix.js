window.autofillFix = (function () {
    function dispatchInputEvent(el) {
        try {
            var evt = new Event('input', { bubbles: true });
            el.dispatchEvent(evt);
            var changeEvt = new Event('change', { bubbles: true });
            el.dispatchEvent(changeEvt);
        }
        catch (e) {
            // ignore
        }
    }

    function triggerForIds(ids) {
        if (!ids || !ids.forEach) return;
        ids.forEach(function (id) {
            var el = document.getElementById(id);
            if (el && el.value && el.value.length > 0) {
                dispatchInputEvent(el);
            }
        });
    }

    function triggerForAllInputs() {
        var inputs = document.querySelectorAll('input');
        inputs.forEach(function (el) {
            if (el && el.value && el.value.length > 0) {
                dispatchInputEvent(el);
            }
        });
    }

    function getValues(ids) {
        var result = {};
        if (ids && ids.forEach) {
            ids.forEach(function (id) {
                var el = document.getElementById(id);
                result[id] = el ? el.value : null;
            });
        }
        else {
            var inputs = document.querySelectorAll('input');
            inputs.forEach(function (el) {
                if (el && el.id) result[el.id] = el.value;
            });
        }
        return result;
    }

    function initAutoTrigger() {
        // Run immediately and again after short delays to catch late browser autofill
        try {
            triggerForAllInputs();
            setTimeout(triggerForAllInputs, 300);
            setTimeout(triggerForAllInputs, 1000);
        }
        catch (e) {
            // ignore
        }
    }

    return {
        triggerForIds: triggerForIds,
        triggerForAllInputs: triggerForAllInputs,
        getValues: getValues,
        initAutoTrigger: initAutoTrigger
    };
})();
