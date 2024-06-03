// Function to generate level animation
function generatelvl(level, AnimationSpeed, color, HowFarAnimationComes) {
    var canvas = document.querySelector('#faceitlvl');
    var c = canvas.getContext('2d');
    const centerX = canvas.width / 2;
    const centerY = canvas.height / 2;
    const circle = canvas.width / 2;

    var x = -3.983;
    var dx = AnimationSpeed;

    // Draw outer circle
    c.beginPath();
    c.arc(centerX, centerY, circle, 0, 2 * Math.PI);
    c.strokeStyle = "#1F1F22";
    c.fillStyle = "#1F1F22";
    c.fill();
    c.stroke();

    // Draw inner circle background
    c.beginPath();
    c.arc(centerX, centerY, circle * 0.7, 2.3, 0.90);
    c.lineWidth = circle * 0.2;
    c.strokeStyle = "RGB(205, 205, 205,0.1)";
    c.fill();
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

document.addEventListener("DOMContentLoaded", function () {
    const input = document.getElementById("animatedInput");
    const placeholderText = "Enter your nickname. . .";
    let currentIndex = 0;
    let showCursor = true;

    function type() {
        let placeholder = placeholderText.substring(0, currentIndex);
        if (currentIndex < placeholderText.length) {
            placeholder += "|"; // Add cursor if typing is still ongoing
        }
        input.setAttribute("placeholder", placeholder);
        if (currentIndex < placeholderText.length) {
            currentIndex++;
            setTimeout(type, 60); // Adjust the speed of typing here
        } else {
            showCursor = true; // Start blinking cursor after typing is completed
            blinkCursor(); // Start blinking cursor
        }
    }

    function blinkCursor() {
        input.setAttribute("placeholder", placeholderText + (showCursor ? "|" : ""));
        showCursor = !showCursor;
        setTimeout(blinkCursor, 800); // Adjust the speed of blinking here
    }

    type(); // Start typing immediately
});



