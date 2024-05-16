document.addEventListener('DOMContentLoaded', function () {
    const ws = new WebSocket('ws://localhost:3000');

    // Function to display messages in the chat interface
    function displayMessage(message, type) {
        const chatContainer = document.querySelector('.chat-container');
        const messageDiv = document.createElement('div');
        messageDiv.classList.add('message');
        
        if (type === 'user') {
            messageDiv.classList.add('user');
        } else {
            messageDiv.classList.add('twined');
        }

        messageDiv.textContent = message;
        chatContainer.appendChild(messageDiv);
        chatContainer.scrollTop = chatContainer.scrollHeight;  // Auto-scroll to the newest message
    }

    // Function to send command to server
    function sendCommand() {
        const input = document.querySelector('.command-line-input');
        const command = input.value.trim();
        if (command) {
            displayMessage(command, 'user');  // Display user's command in chat
            ws.send(command);
            input.value = ''; // Clear input after sending
        }
    }

    // WebSocket Event Handlers
    ws.onopen = function() {
        console.log("Connected to WebSocket");
    };

    ws.onerror = function(error) {
        console.error("WebSocket Error: ", error);
        displayMessage("Error connecting to server.", 'system');
    };

    ws.onclose = function() {
        console.log("WebSocket connection closed");
        displayMessage("Connection closed.", 'system');
    };

    // Handle messages received from server
    ws.onmessage = function(event) {
        console.log("Received data: ", event.data);
        displayMessage(event.data, 'twined'); 
    };

    // Event listeners for UI interaction
    document.querySelector('.send-button').addEventListener('click', sendCommand);
    document.querySelector('.command-line-input').addEventListener('keypress', function(event) {
        if (event.key === 'Enter') {
            event.preventDefault();
            sendCommand();
        }
    });
});
