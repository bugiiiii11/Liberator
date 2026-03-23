mergeInto(LibraryManager.library, {
    SendCombatResult: function(jsonResultPtr) {
        var jsonResult = UTF8ToString(jsonResultPtr);
        if (window.onUnityCombatResult) {
            window.onUnityCombatResult(JSON.parse(jsonResult));
        }
        // Also dispatch a custom event for React to listen to
        window.dispatchEvent(new CustomEvent('liberator-combat-result', { detail: JSON.parse(jsonResult) }));
    },

    SendGameReady: function() {
        if (window.onUnityGameReady) {
            window.onUnityGameReady();
        }
        window.dispatchEvent(new CustomEvent('liberator-game-ready'));
    },

    SendGameError: function(errorMessagePtr) {
        var errorMessage = UTF8ToString(errorMessagePtr);
        if (window.onUnityGameError) {
            window.onUnityGameError(errorMessage);
        }
        window.dispatchEvent(new CustomEvent('liberator-game-error', { detail: errorMessage }));
    },

    RequestGameClose: function() {
        if (window.onUnityRequestClose) {
            window.onUnityRequestClose();
        }
        window.dispatchEvent(new CustomEvent('liberator-request-close'));
    }
});
