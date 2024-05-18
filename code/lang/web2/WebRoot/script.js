document.addEventListener("DOMContentLoaded", function() {
    const commandInput = document.querySelector(".command-line-input");
    const sendButton = document.querySelector(".send-button");
    const chatContainer = document.querySelector(".chat-container");
    const svgDisplay = document.getElementById("svg-display");
    const graphContent = document.getElementById("graph-content");
    const textDisplay = document.getElementById("text-display");

    sendButton.addEventListener("click", function() {
        const userPrompt = commandInput.value.trim();
        
        if (userPrompt.startsWith("TwinedChat:")) {
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;
            chatContainer.appendChild(userMessage);

            fetch("/api/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ userPrompt: userPrompt })
            })
            .then(response => response.text())
            .then(data => {
                const aiMessage = document.createElement("div");
                aiMessage.className = "message twined";
                aiMessage.textContent = data;
                chatContainer.appendChild(aiMessage);
                commandInput.value = "";
            })
            .catch(error => {
                console.error("Error:", error);
            });
        } else if (userPrompt.startsWith("TwinedGraph:")) {
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;
            chatContainer.appendChild(userMessage);

            fetch("/api/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ userPrompt: userPrompt })
            })
            .then(response => response.json())
            .then(data => {
                const aiMessage = document.createElement("div");
                aiMessage.className = "message twined";
                aiMessage.innerHTML = `<strong>${data.message}</strong><br><pre>${data.content}</pre>`;
                chatContainer.appendChild(aiMessage);
                commandInput.value = "";
            })
            .catch(error => {
                console.error("Error:", error);
            });
        } else if (userPrompt.startsWith("TwinedOpSVG:")) {
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;
            chatContainer.appendChild(userMessage);

            fetch("/api/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ userPrompt: userPrompt })
            })
            .then(response => response.json())
            .then(data => {
                if (userPrompt.trim() === "TwinedOpSVG:") {
                    // Display the list of SVG files
                    const aiMessage = document.createElement("div");
                    aiMessage.className = "message twined";
                    aiMessage.innerHTML = `<strong>SVG Files:</strong><br><pre>${data}</pre>`;
                    chatContainer.appendChild(aiMessage);
                } else {
                    // Display the content of the specific SVG file
                    svgDisplay.innerHTML = data;
                }
                commandInput.value = "";
            })
            .catch(error => {
                console.error("Error:", error);
            });
        } else if (userPrompt.startsWith("TwinedText:")) {
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;
            chatContainer.appendChild(userMessage);

            fetch("/api/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ userPrompt: userPrompt })
            })
            .then(response => response.json())
            .then(data => {
                const aiMessage = document.createElement("div");
                aiMessage.className = "message twined";
                aiMessage.innerHTML = `<strong>${data.message}</strong><br><pre>${data.content}</pre>`;
                chatContainer.appendChild(aiMessage);
                commandInput.value = "";
            })
            .catch(error => {
                console.error("Error:", error);
            });
        } else if (userPrompt.startsWith("TwinedOpText:")) {
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;
            chatContainer.appendChild(userMessage);

            fetch("/api/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ userPrompt: userPrompt })
            })
            .then(response => response.json())
            .then(data => {
                if (userPrompt.trim() === "TwinedOpText:") {
                    // Display the list of text files
                    const aiMessage = document.createElement("div");
                    aiMessage.className = "message twined";
                    aiMessage.innerHTML = `<strong>Text Files:</strong><br><pre>${data}</pre>`;
                    chatContainer.appendChild(aiMessage);
                } else {
                    // Display the content of the specific text file
                    textDisplay.innerHTML = `<pre>${data}</pre>`;
                }
                commandInput.value = "";
            })
            .catch(error => {
                console.error("Error:", error);
            });
        }
    });

    // Fetch and display the content of the graph
    fetch("/displayContent")
        .then(response => response.json())
        .then(data => {
            graphContent.textContent = data;
        })
        .catch(error => {
            console.error("Error fetching graph content:", error);
            graphContent.textContent = "Error loading content.";
        });
});
