
console.log("TicTacToe module loaded");

//$(function () {
//    var gameName = $('#game-name').text();
//    if (gameName !== "Tic-Tac-Toe") return;

//    console.log("Initializing Tic-Tac-Toe game");

//    let currentPlayer = null;
//    let mySymbol = null;
//    let board = Array(9).fill(' ');
//    let opponentId = null;
//    let isComputerOpponent = false;
//    let gameOver = false;
//    let userId = $('input[name="user-id"]').val();
//    let gameStats = { wins: 0, losses: 0, ties: 0, total: 0 };

//    loadGameStats();

//    function loadGameStats() {
//        $.ajax({
//            url: '/Games/GetGameStats',
//            type: 'GET',
//            data: { gameId: 1 },
//            success: function (data) {
//                if (data.success) {
//                    gameStats = data.stats;
//                    updateStatsDisplay();
//                }
//            },
//            error: function (err) {
//                console.error("Error loading game stats:", err);
//            }
//        });
//    }

//    function updateStatsDisplay() {
//        $('#game-stats').html(`
//            <h5>Tic Tac Toe Stats</h5>
//            <table class="table table-sm">
//                <tr>
//                    <td>Total Games</td>
//                    <td>${gameStats.total}</td>
//                </tr>
//                <tr>
//                    <td>Wins</td>
//                    <td>${gameStats.wins}</td>
//                </tr>
//                <tr>
//                    <td>Losses</td>
//                    <td>${gameStats.losses}</td>
//                </tr>
//                <tr>
//                    <td>Ties</td>
//                    <td>${gameStats.ties}</td>
//                </tr>
//            </table>

//            <h6 class="mt-3">This Session:</h6>
//            <table class="table table-sm">
//                <tr>
//                    <td>Games</td>
//                    <td id="session-games">0</td>
//                </tr>
//                <tr>
//                    <td>Wins</td>
//                    <td id="session-wins">0</td>
//                </tr>
//                <tr>
//                    <td>Losses</td>
//                    <td id="session-losses">0</td>
//                </tr>
//                <tr>
//                    <td>Ties</td>
//                    <td id="session-ties">0</td>
//                </tr>
//            </table>
//        `);
//    }

//    $('#play-again-btn').click(function () {
//        resetGame();
//    });

//    $('#exit-game-btn').click(function () {
//        if (!gameOver && !confirm("Are you sure you want to exit the game? This will count as a loss.")) {
//            return;
//        }
//        window.location.href = '/Games';
//    });

//    $('#playAgainModalBtn').click(function () {
//        $('#playAgainModal').modal('hide');
//        resetGame();
//    });

//    $('#exitModalBtn').click(function () {
//        window.location.href = '/Games';
//    });

//    window.makeMove = function (index) {
//        console.log("Move requested at index " + index);
//    };

//    function resetGame() {
//        board = Array(9).fill(' ');
//        gameOver = false;
//        currentPlayer = 'X';

//        $('.game-cell').each(function () {
//            $(this).text('');
//            $(this).prop('disabled', false);
//            $(this).removeClass('x-mark o-mark winner');
//        });

//        $('#game-result').html('');
//        $('#play-again-btn').hide();

//        const statusMessage = $('#game-status-message');
//        statusMessage.removeClass('alert-success alert-danger alert-warning').addClass('alert-info');

//        if (isComputerOpponent) {
//            statusMessage.text("New game started. X goes first.");
//            mySymbol = 'X';
//            $('#my-symbol').text('X');
//            updateCurrentTurnDisplay();
//        } else {
//            window.location.reload();
//        }
//    }

//    function updateCurrentTurnDisplay() {
//        const turnDisplay = $('#current-turn');
//        turnDisplay.text(currentPlayer);
//        turnDisplay.removeClass('text-success text-danger');
//        turnDisplay.addClass(currentPlayer === mySymbol ? 'text-success' : 'text-danger');

//        if (currentPlayer === mySymbol) {
//            $('#game-status-message').text("Your turn");
//        } else {
//            $('#game-status-message').text("Opponent's turn");
//        }
//    }
//});