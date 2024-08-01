document.addEventListener("DOMContentLoaded", function () {
    // Typing effect for the input placeholder
    function animateInputPlaceholder() {
        const input = document.getElementById("animatedInput");
        const placeholderText = "Enter your nickname. . .";
        let currentIndex = 0;
        let showCursor = true;

        function typeInput() {
            let placeholder = placeholderText.substring(0, currentIndex);
            if (currentIndex < placeholderText.length) {
                placeholder += "|"; // Add cursor if typing is still ongoing
            }
            input.setAttribute("placeholder", placeholder);
            if (currentIndex < placeholderText.length) {
                currentIndex++;
                setTimeout(typeInput, 60); // Adjust the speed of typing here
            } else {
                showCursor = true; // Start blinking cursor after typing is completed
                blinkInputCursor(); // Start blinking cursor
            }
        }

        function blinkInputCursor() {
            input.setAttribute("placeholder", placeholderText + (showCursor ? "|" : ""));
            showCursor = !showCursor;
            setTimeout(blinkInputCursor, 800); // Adjust the speed of blinking here
        }

        typeInput(); // Start typing immediately
    }

    // Typing effect for the info text
    function animateInfoText() {
        const info = document.getElementById("animatedInfo");
        const infoText = "You can also paste a Steam profile link to find the connected FACEIT account.";
        let infoIndex = 0;
        let showCursor = true;

        function typeInfo() {
            let displayedText = infoText.substring(0, infoIndex);
            if (infoIndex < infoText.length) {
                displayedText += "|"; // Add cursor if typing is still ongoing
            }
            info.textContent = displayedText;
            if (infoIndex < infoText.length) {
                infoIndex++;
                setTimeout(typeInfo, 19); // Adjust the speed of typing here
            } else {
                showCursor = true; // Start blinking cursor after typing is completed
                blinkInfoCursor(); // Start blinking cursor
            }
        }

        typeInfo(); // Start typing immediately
    }

    // Call both functions to start animations
    animateInputPlaceholder();
    // Delay starting the info text typing until the input placeholder finishes typing
    animateInfoText();
});
