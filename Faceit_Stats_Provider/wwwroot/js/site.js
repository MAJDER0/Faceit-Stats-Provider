// Function to generate level animation
function generatelvl(level, AnimationSpeed, color, HowFarAnimationComes) {
    var canvas = document.querySelector('#faceitlvl');
    var c = canvas.getContext('2d');
    const centerX = canvas.width / 2 ;
    const centerY = canvas.height / 2 ;
    const circle = canvas.width / 2 -1;

    var x = -3.974;
    var dx = AnimationSpeed;

    // Draw outer circle
    c.beginPath();
    c.arc(centerX, centerY, circle, 0,  2* Math.PI);
    c.strokeStyle = "rgba(31, 31, 34, 1)";
    c.fillStyle = "rgba(31, 31, 34, 0.2)";
    c.fill();
    c.stroke();

    // Draw inner circle background
    c.beginPath();
    c.arc(centerX, centerY, circle * 0.7, 2.3, 0.90);
    c.lineWidth = circle * 0.2;
    c.strokeStyle = "rgba(205, 205, 205,0.1";
    c.stroke();


    // Draw progress arc
    c.beginPath();
    c.arc(centerX, centerY, circle * 0.7, 2.3, x);
    c.lineWidth = circle * 0.2;
    c.strokeStyle = color;
    c.fill();
    c.stroke();


    // Draw level text
    c.beginPath();
    c.font = circle * 0.70 + "px 'Play', sans-serif";
    c.textBaseline = "middle";
    c.textAlign = "center";
    c.lineWidth = circle * 0.04;
    c.strokeText(level, centerX - circle * 0.035, centerY);
    c.fillStyle = color;
    c.fillText(level, centerX - circle * 0.035, centerY);
    c.fill();
    c.stroke();

    // Animation function
    function animate() {
        c.beginPath();
        c.lineWidth = circle * 0.2;
        c.arc(centerX, centerY, circle * 0.7, 2.3, x);
        c.stroke();


        if (x <= HowFarAnimationComes) {
            x += dx;
            requestAnimationFrame(animate);
        }
    }
    animate();
}

// Function to generate Elo graph
function generateEloGraph() {
    const ctx = document.getElementById('EloChart');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: ['Red', 'Blue', 'Yellow', 'Green', 'Purple', 'Orange'],
            datasets: [{
                label: '# of Votes',
                data: [12, 19, 3, 5, 2, 3],
                borderWidth: 1
            }]
        },
        options: {
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

$(document).on('click', '.match-row-second', function () {
        
    $(this).next(".scoreboard").toggle();

});




