// Wait for the DOM to be fully loaded before running the script
document.addEventListener("DOMContentLoaded", function() {

    // Select the input field and send button
    const commandInput = document.querySelector(".command-line-input");
    const sendButton = document.querySelector(".send-button");
    const chatContainer = document.querySelector(".chat-container");

    // Add an event listener to the send button for the click event
    sendButton.addEventListener("click", function() {

        // Get the user's input and trim any whitespace
        const userPrompt = commandInput.value.trim();
        
        // Check if the user input starts with "TwinedChat:"
        if (userPrompt.startsWith("TwinedChat:")) {

            // Create a div element for the user's message
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;

            // Append the user's message to the chat container
            chatContainer.appendChild(userMessage);

            // Send a POST request to the server with the user's prompt
            fetch("/api/chat", {
                method: "POST", // HTTP method
                headers: {
                    "Content-Type": "application/json" // Content type
                },
                body: JSON.stringify({ userPrompt: userPrompt }) // Request body as JSON
            })

            // Convert the response to text
            .then(response => response.text())
            .then(data => {

                // Create a div element for the AI's response
                const aiMessage = document.createElement("div");
                aiMessage.className = "message twined";
                aiMessage.textContent = data;

                // Append the AI's response to the chat container
                chatContainer.appendChild(aiMessage);

                // Clear the input field
                commandInput.value = "";
            })
            
            // Catch any errors that occur during the fetch request
            .catch(error => {
                console.error("Error:", error);
            });
        }
    });
});
