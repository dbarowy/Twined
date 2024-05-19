document.addEventListener("DOMContentLoaded", function() {
    const commandInput = document.querySelector(".command-line-input");
    const sendButton = document.querySelector(".send-button");
    const chatContainer = document.querySelector(".chat-container");
    const svgDisplay = document.getElementById("svg-display");
    const graphContent = document.getElementById("graph-content");
    const textDisplay = document.getElementById("text-display");

    sendButton.addEventListener("click", function() {
        const userPrompt = commandInput.value.trim();
        

        // calling main method
        if (userPrompt.startsWith("Start")|| userPrompt.startsWith("start") || userPrompt.startsWith("Start:")) {
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;
            chatContainer.appendChild(userMessage);

            fetch("/api/callMain", {
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

        // continues conversation with main
         } else if (userPrompt.startsWith("1") || userPrompt.startsWith("2")) {
                const userMessage = document.createElement("div");
                userMessage.className = "message user";
                userMessage.textContent = userPrompt;
                chatContainer.appendChild(userMessage);
    
                fetch("/api/find", {
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

        } else if (userPrompt.startsWith("3")) {
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;
            chatContainer.appendChild(userMessage);

            const handleRequests = async () => {
                try {
                    // First fetch request
                    let response = await fetch("/api/path", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify({ userPrompt: userPrompt })
                    });

                    let data = await response.json();

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
                    
                    // need this or the file is not updated by the time we post it
                    await new Promise(resolve => setTimeout(resolve, 25));

                    // Second fetch request
                    response = await fetch("/api/disp_text", {
                        method: "POST",
                        headers: {
                            "Content-Type": "application/json"
                        },
                        body: JSON.stringify({ userPrompt: "" })
                    });

                    data = await response.json();

                    textDisplay.innerHTML = `<pre>${data}</pre>`;
                    commandInput.value = "";

                } catch (error) {
                    console.error("Error:", error);
                }
            };

            handleRequests();


        // used to call the chat api and return a the expanded information
         } else if (userPrompt.startsWith("expand") || userPrompt.startsWith("Expand") || userPrompt.startsWith("Expand:")) {
            const userMessage = document.createElement("div");
            userMessage.className = "message user";
            userMessage.textContent = userPrompt;
            chatContainer.appendChild(userMessage);
        
            // Initial fetch request to expand on the graph
            fetch("/api/chat", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({ userPrompt: userPrompt })
            })
            .then(response => response.json())
            .then(data => {
                userMessage.textContent = data;
        
            })
            .then(() => {
                // First fetch request to get the SVG or list of SVG files
                return fetch("/api/path", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({ userPrompt: userPrompt })
                });
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
        
            })
            .then(() => {
                // Second fetch request to get the updated graph information
                return fetch("/api/disp_text", {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json"
                    },
                    body: JSON.stringify({ userPrompt: "" })
                });
            })
            .then(response => response.json())
            .then(data => {
                textDisplay.innerHTML = `<pre>${data}</pre>`;
                commandInput.value = "";
            })
            .catch(error => {
                console.error("Error:", error);
            });
        }

        // } else if (userPrompt.startsWith("expand") || userPrompt.startsWith("Expand")) {
        //     const userMessage = document.createElement("div");
        //     userMessage.className = "message user";
        //     userMessage.textContent = userPrompt;
        //     chatContainer.appendChild(userMessage);
        
        //     const handleRequests = async () => {
        //         try {
        //             // Initial fetch request to expand the graph
        //             let response = await fetch("/api/update", {
        //                 method: "POST",
        //                 headers: {
        //                     "Content-Type": "application/json"
        //                 },
        //                 body: JSON.stringify({ userPrompt: userPrompt })
        //             });
        
        //             let data = await response.json();
        //             userMessage.textContent = data;
        
        //             // Short delay to ensure the update is processed
        //             await new Promise(resolve => setTimeout(resolve, 1000));
        
        //             // First fetch request to get the SVG or list of SVG files
        //             response = await fetch("/api/path", {
        //                 method: "POST",
        //                 headers: {
        //                     "Content-Type": "application/json"
        //                 },
        //                 body: JSON.stringify({ userPrompt: userPrompt })
        //             });
        
        //             data = await response.json();
        
        //             if (userPrompt.trim() === "TwinedOpSVG:") {
        //                 // Display the list of SVG files
        //                 const aiMessage = document.createElement("div");
        //                 aiMessage.className = "message twined";
        //                 aiMessage.innerHTML = `<strong>SVG Files:</strong><br><pre>${data}</pre>`;
        //                 chatContainer.appendChild(aiMessage);
        //             } else {
        //                 // Display the content of the specific SVG file
        //                 svgDisplay.innerHTML = data;
        //             }
        
        //             // Another short delay to ensure the file update is complete
        //             await new Promise(resolve => setTimeout(resolve, 1050));
        
        //             // Second fetch request to get the updated graph information
        //             response = await fetch("/api/disp_text", {
        //                 method: "POST",
        //                 headers: {
        //                     "Content-Type": "application/json"
        //                 },
        //                 body: JSON.stringify({ userPrompt: "" })
        //             });
        
        //             data = await response.json();
        
        //             textDisplay.innerHTML = `<pre>${data}</pre>`;
        //             commandInput.value = "";
        
        //         } catch (error) {
        //             console.error("Error:", error);
        //         }
        //     };
        
        //     handleRequests();
        
        // } 
        
        else if (userPrompt.startsWith("TwinedOpSVG:")) {
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
