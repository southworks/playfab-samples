userTeamManagement = (() => {
    const starterTableId = 'starters-table-body';
    const subsTableId = 'subs-table-body';
    const starterButtonContent = 'Start';
    const benchButtonContent = 'Bench';
    const swapPlayerButtonCssSelector = '.btn.btn-southworks.btn-swap-player';

    function setSaveButtonDisabled(value) {
        if (value) {
            $("#btn-save-user-team-changes").addClass("disabled")
        } else {
            $("#btn-save-user-team-changes").removeClass("disabled")
        }
    }

    function retrieveCurrentUserTeam() {
        let id = $("#user-team-id").html();
        let players = [];

        $("#starters-table-body").find("tr").each(function (_, row) {
            players.push({
                InventoryId: row.dataset.futbolPlayerId,
                IsStarter: true
            })
        });
        $("#subs-table-body").find("tr").each(function (_, row) {
            players.push({
                InventoryId: row.dataset.futbolPlayerId,
                IsStarter: false
            })
        });

        return {
            ID: id,
            Players: players,
        };
    }

    return {
        handleSwapPlayer: (event) => {
            let row = event.target.parentElement.parentElement;
            let sourceId = row.parentElement.id;

            let targetId = '';
            let buttonText = '';
            if (sourceId === starterTableId) {
                targetId = subsTableId
                buttonText = starterButtonContent;
            } else {
                targetId = starterTableId
                buttonText = benchButtonContent;
            }
            
            $(row).find(swapPlayerButtonCssSelector).html(buttonText);
            $(row).remove();
            $(`#${targetId}`).html(row.outerHTML + $(`#${targetId}`).html());
            setSaveButtonDisabled(false);
        },
        saveUserTeam: (modalId) => {
            $.ajax(
                {
                    method: 'POST',
                    url: '/userteammanagement/updateuserteam',
                    data: retrieveCurrentUserTeam(),
                    success: function (response) {
                        var body = '';
                        switch (response.statusCode) {
                            case 200:
                                body = 'Changes saved';
                                setSaveButtonDisabled(true);
                                break;
                            case 400:
                                body = response.message;
                                setSaveButtonDisabled(true);
                                break;
                            case 404:
                            case 500:
                                body = 'Error saving changes, try again in a few moments';
                                break;
                        }

                        setCustomModalData(modalId, "Team management", body);
                        showCustomModal(modalId);
                    },
                    error: function (jqXHR, textStatus, _) {
                        showAlert('Error - Code: ' + jqXHR.status + ' - Message: ' + textStatus, alertDangerType);
                    }
                }
            )
        },
        sellPlayer: (event, playerId, modalId) => {
            marketplace.sellPlayer(playerId, modalId, () => {
                let row = event.target.parentElement.parentElement;
                let sourceId = row.parentElement.id;
                let buttonText = sourceId === starterTableId ? starterButtonContent : benchButtonContent;
                $(row).find(swapPlayerButtonCssSelector).html(buttonText);
                $(row).remove();

                var tableStarters = document.getElementById("table-starters");
                var tableSubs = document.getElementById("table-subs");
                if (tableStarters && tableSubs) {
                    var countStarters = tableStarters.tBodies[0].rows.length;
                    var countSubs = tableSubs.tBodies[0].rows.length;
                    if (countStarters == 0 && countSubs == 0) {
                        var teamWithPlayers = document.getElementById("team-with-players");
                        var teamEmpty = document.getElementById("team-empty");
                        if (teamWithPlayers && teamEmpty) {
                            teamWithPlayers.style.display = "none";
                            teamEmpty.style.display  = 'block';
                        }
                    }
                }
            });
        }
    }
})();
