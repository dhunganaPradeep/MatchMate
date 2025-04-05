$(function () {
    const options = ['Rock', 'Paper', 'Scissors'];
    let sessionStats = {
        games: 0,
        wins: 0,
        losses: 0,
        ties: 0
    };

    loadGameStats();
    setupCustomModal();

    function setupCustomModal() {
        $('#rpsPlayAgainModal').removeClass('modal fade');

        $('#customModalClose, #rpsExitModalBtn').on('click', function () {
            hideCustomModal();
            if ($(this).attr('id') === 'rpsExitModalBtn') {
                window.location.href = '/Games/Index';
            }
        });

        $('#rpsPlayAgainModalBtn').on('click', function () {
            hideCustomModal();
            document.getElementById('rps-result').innerHTML = '';
        });
        $('#customModalOverlay').on('click', function (e) {
            if (e.target === this) {
                hideCustomModal();
            }
        });

        hideCustomModal();
    }

    function showCustomModal() {
        $('#customModalOverlay').show();
    }

    function hideCustomModal() {
        $('#customModalOverlay').hide();
    }

    function loadGameStats() {
        console.log("Loading RPS game stats...");

        const statsContainerId = "game-stats";
        const statsContainer = document.querySelector('.rps-game-container .col-md-6 #' + statsContainerId);

        if (!statsContainer) {
            console.error("RPS Stats container not found!");
            return;
        }

        console.log("Found RPS stats container:", statsContainer);

        statsContainer.innerHTML = `
            <h5>Rock Paper Scissors Stats</h5>
            <div class="text-center p-3">
                <div class="spinner-border text-primary" role="status">
                    <span class="visually-hidden">Loading...</span>
                </div>
                <p class="mt-2">Loading stats...</p>
            </div>
        `;

        $.ajax({
            url: '/Games/GetGameStats',
            type: 'GET',
            data: { gameId: 2 },
            dataType: 'json',
            success: function (data) {
                console.log("RPS stats loaded successfully:", data);
                if (data && data.success && data.stats) {
                    updateStatsDisplay(data.stats, statsContainer);
                } else {
                    statsContainer.innerHTML = `
                        <h5>Rock Paper Scissors Stats</h5>
                        <div class="alert alert-warning">
                            Unable to load stats. Please try refreshing the page.
                        </div>
                    `;
                }
            },
            error: function (xhr, status, error) {
                console.error("Error loading RPS stats:", error);
                statsContainer.innerHTML = `
                    <h5>Rock Paper Scissors Stats</h5>
                    <div class="alert alert-danger">
                        Error loading stats. Please try again.
                    </div>
                `;
            }
        });
    }

    function updateStatsDisplay(stats, container) {
        console.log("Updating RPS stats display with:", stats);

        if (!container) {
            container = document.querySelector('.rps-game-container .col-md-6 #game-stats');
            if (!container) {
                console.error("Stats container not found for update!");
                return;
            }
        }

        container.innerHTML = `
            <h5>Rock Paper Scissors Stats</h5>
            <table class="table table-sm">
                <tr>
                    <td>Total Games</td>
                    <td>${stats.total || 0}</td>
                </tr>
                <tr>
                    <td>Wins</td>
                    <td>${stats.wins || 0}</td>
                </tr>
                <tr>
                    <td>Losses</td>
                    <td>${stats.losses || 0}</td>
                </tr>
                <tr>
                    <td>Ties</td>
                    <td>${stats.ties || 0}</td>
                </tr>
            </table>
        `;

        console.log("RPS stats display updated successfully");
    }

    window.playRPS = function (playerChoice) {
        $(`button[onclick="playRPS('${playerChoice}')"]`).addClass('active-choice');
        setTimeout(() => {
            $(`button[onclick="playRPS('${playerChoice}')"]`).removeClass('active-choice');
        }, 1000);

        const computerChoice = options[Math.floor(Math.random() * 3)];
        let result = '';
        let resultType = '';
        let alertClass = '';

        if (playerChoice === computerChoice) {
            result = "It's a tie!";
            resultType = "tie";
            alertClass = "alert-warning";
            sessionStats.ties++;
        } else if (
            (playerChoice === 'Rock' && computerChoice === 'Scissors') ||
            (playerChoice === 'Paper' && computerChoice === 'Rock') ||
            (playerChoice === 'Scissors' && computerChoice === 'Paper')
        ) {
            result = `You win! ${playerChoice} beats ${computerChoice}.`;
            resultType = "win";
            alertClass = "alert-success";
            sessionStats.wins++;
        } else {
            result = `You lose! ${computerChoice} beats ${playerChoice}.`;
            resultType = "lose";
            alertClass = "alert-danger";
            sessionStats.losses++;
        }

        sessionStats.games++;

        document.getElementById('rps-result').innerHTML = `
            <div class="alert ${alertClass}">
                <h4>${result}</h4>
                <div class="d-flex justify-content-center gap-5 mt-3">
                    <div class="text-center">
                        <div class="h3">You</div>
                        <div class="display-4"><i class="fas fa-hand-${playerChoice.toLowerCase()}"></i></div>
                        <div>${playerChoice}</div>
                    </div>
                    <div class="text-center">
                        <div class="h3">Computer</div>
                        <div class="display-4"><i class="fas fa-hand-${computerChoice.toLowerCase()}"></i></div>
                        <div>${computerChoice}</div>
                    </div>
                </div>
            </div>
        `;

        document.getElementById('rps-games').innerText = sessionStats.games;
        document.getElementById('rps-wins').innerText = sessionStats.wins;
        document.getElementById('rps-losses').innerText = sessionStats.losses;
        document.getElementById('rps-ties').innerText = sessionStats.ties;

        saveGameResult(resultType);

        setTimeout(() => {
            document.getElementById('customModalContent').innerHTML = `
                <div class="text-center">
                    <div class="alert ${alertClass} mb-3">
                        <h3>${result}</h3>
                        <div class="d-flex justify-content-center gap-5 mt-3">
                            <div class="text-center">
                                <div class="h4">You</div>
                                <div class="h1"><i class="fas fa-hand-${playerChoice.toLowerCase()}"></i></div>
                            </div>
                            <div class="text-center">
                                <div class="h4">Computer</div>
                                <div class="h1"><i class="fas fa-hand-${computerChoice.toLowerCase()}"></i></div>
                            </div>
                        </div>
                    </div>
                    <p>Would you like to play again?</p>
                </div>
            `;
            showCustomModal();
        }, 1500);
    };

    function saveGameResult(result) {
        const token = document.querySelector('input[name="__RequestVerificationToken"]').value;

        $.ajax({
            url: '/Games/SaveRPSResult',
            type: 'POST',
            data: {
                result: result,
                __RequestVerificationToken: token
            },
            success: function (data) {
                if (data.success) {
                    console.log("Game result saved successfully");
                    loadGameStats(); // Reload stats after saving
                } else {
                    console.error("Error saving game result:", data.message);
                }
            },
            error: function (xhr, status, error) {
                console.error("AJAX Error:", error);
            }
        });
    }
});