marketplace = (() => {
    function manageMarketPlaceResponse(response, modalId, cb) {
        var title = "";
        var body = "";

        switch (response.statusCode) {
            // todo: implement rest of possible StatusCode cases
            case 200:                
                title = "Marketplace - Success";
                if (cb) cb();
                if (document.getElementById("budget")) {
                    document.getElementById("budget").textContent = response.response;
                }
                break;
            case 400:
            case 404:
            case 500:
                title = "Marketplace - Error";
                break;
            default:
                break;
        }

        if (response.message) {
            body += response.message;
        }

        setCustomModalData(modalId, title, body);
        showCustomModal(modalId);
    }
    return {
        buyPlayer: (player, modalId) => {
            $.ajax({
                type: "POST",
                url: "/marketplace/buy",
                data: player,
                success: response => manageMarketPlaceResponse(response, modalId, () => {
                    document.getElementById(player.id).style.display = 'none';
                }),
                error: (response) => {
                    setCustomModalData(modalId, 'Marketplace - Buy player Failed', response.message || '')
                    showCustomModal(modalId);
                }
            });
        },
        sellPlayer: (playerId, modalId, cb) => {
            $.ajax(
                {
                    method: 'POST',
                    url: '/marketplace/sell',
                    data: {
                        id: playerId
                    },
                    success: response => manageMarketPlaceResponse(response, modalId, cb),
                    error: function (jqXHR, textStatus, _) {
                        setCustomModalData(modalId, 'Marketplace - Sell player Failed', response.message || '')
                        showCustomModal(modalId);
                    }
                }
            )
        }
    }
})();
