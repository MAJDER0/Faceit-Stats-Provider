
document.addEventListener("DOMContentLoaded", function () {
    const input = document.getElementById("SearchBarAnalyzer");
    const placeholderText = "Enter Room ID. . .";
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