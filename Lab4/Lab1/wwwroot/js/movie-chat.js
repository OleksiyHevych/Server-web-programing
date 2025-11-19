"use strict";

const movieIdEl = document.getElementById("movieId");
const messageInput = document.getElementById("messageInput");
const sendBtn = document.getElementById("sendBtn");
const fileInput = document.getElementById("fileInput");
const messagesContainer = document.getElementById("messages");
const onlineUsersEl = document.getElementById("onlineUsers");

const currentUserId = document.getElementById('currentUserId')?.value || '';
const currentUserName = document.getElementById('currentUserName')?.value || '';

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/movieChatHub")
    .withAutomaticReconnect()
    .build();


//
// --- Додавання повідомлень у Movie Chat
//
function appendMessage(m) {
    if (!messagesContainer) return;

    const node = document.createElement("div");
    node.className = m.senderName === currentUserName ? "text-end mb-2" : "text-start mb-2";

    const fileHtml = m.fileUrl
        ? `<div><a href="${m.fileUrl}" target="_blank">${m.fileName || 'file'}</a></div>`
        : '';

    node.innerHTML = `
        <strong>${m.senderName}</strong>: ${m.text || ''} 
        ${fileHtml}
        <small class="text-muted">${new Date(m.createdAt).toLocaleString()}</small>
    `;

    messagesContainer.appendChild(node);
    messagesContainer.scrollTop = messagesContainer.scrollHeight;
}



//
// --- Керування списком онлайн-користувачів
//
function addOnlineUser(userId, userName) {
    if (!onlineUsersEl.querySelector(`#user-${userId}`)) {
        const li = document.createElement("li");
        li.id = `user-${userId}`;
        li.className = "list-group-item d-flex justify-content-between align-items-center";
        li.innerHTML = `${userName} <button class="btn btn-sm btn-outline-primary btn-dm">DM</button>`;
        li.querySelector(".btn-dm").addEventListener("click", () => openDMWindow(userId, userName));

        onlineUsersEl.appendChild(li);
    }
}

function removeOnlineUser(userId) {
    const el = document.getElementById(`user-${userId}`);
    if (el) el.remove();
}



//
// --- DM-вікно з нормальним resize (вправо + вниз)
//
function openDMWindow(userId, userName) {
    if (document.getElementById(`dm-${userId}`)) {
        renderPrivateMessages(userId);
        return;
    }

    // === Створення вікна ===
    const wrapper = document.createElement("div");
    wrapper.id = `dm-${userId}`;
    wrapper.className = "dm-window border p-2 bg-light position-fixed";
    wrapper.style.top = "100px";
    wrapper.style.left = `${100 + document.querySelectorAll('.dm-window').length * 260}px`;
    wrapper.style.width = "260px";
    wrapper.style.height = "300px";
    wrapper.style.boxShadow = "0 2px 8px rgba(0,0,0,0.25)";
    wrapper.style.resize = "both";
    wrapper.style.overflow = "hidden";
    wrapper.style.display = "flex";
    wrapper.style.flexDirection = "column";

    wrapper.innerHTML = `
        <div class="dm-header" id="dmHeader-${userId}">
            <strong>${userName}</strong>
            <div>
                <button class="btn btn-sm btn-light" id="toggleDM-${userId}">🗕</button>
                <button class="btn btn-sm btn-light" id="closeDM-${userId}">✖</button>
            </div>
        </div>
        <div class="dm-body" id="dmBody-${userId}">
            <div class="dm-messages" id="dmMessages-${userId}"></div>
            <div class="dm-input-group">
                <input type="text" id="dmInput-${userId}" class="form-control form-control-sm" placeholder="Написати..." />
                <button class="btn btn-primary btn-sm" id="dmSend-${userId}">Відправити</button>
            </div>
        </div>
    `;


    document.body.appendChild(wrapper);


    //
    // --- DRAG
    //
    const header = document.getElementById(`dmHeader-${userId}`);
    let dragging = false, offsetX = 0, offsetY = 0;

    header.addEventListener("mousedown", (e) => {
        dragging = true;
        offsetX = e.clientX - wrapper.offsetLeft;
        offsetY = e.clientY - wrapper.offsetTop;
        header.style.cursor = "grabbing";
    });

    document.addEventListener("mousemove", (e) => {
        if (!dragging) return;
        wrapper.style.left = (e.clientX - offsetX) + "px";
        wrapper.style.top = (e.clientY - offsetY) + "px";
    });

    document.addEventListener("mouseup", () => {
        dragging = false;
        header.style.cursor = "grab";
    });




    //
    // --- Toggle
    //
    const dmBody = document.getElementById(`dmBody-${userId}`);
    const toggleBtn = document.getElementById(`toggleDM-${userId}`);

    toggleBtn.addEventListener("click", () => {
        const collapsed = dmBody.style.display === "none";
        dmBody.style.display = collapsed ? "flex" : "none";
        toggleBtn.textContent = collapsed ? "🗕" : "🗖";
    });


    //
    // --- Close
    //
    document.getElementById(`closeDM-${userId}`).addEventListener("click", () => wrapper.remove());



    //
    // --- Send
    //
    const input = document.getElementById(`dmInput-${userId}`);
    const sendBtn = document.getElementById(`dmSend-${userId}`);

    sendBtn.addEventListener("click", () => {
        const text = input.value.trim();
        if (!text) return;

        input.value = "";
        connection.invoke("SendPrivateMessage", userId, text).catch(console.error);
    });

    input.addEventListener("keydown", (e) => {
        if (e.key === "Enter") {
            e.preventDefault();
            sendBtn.click();
        }
    });

    renderPrivateMessages(userId);
}



//
// --- Рендер історії
//
function renderPrivateMessages(userId) {
    const dmMessages = document.getElementById(`dmMessages-${userId}`);
    if (!dmMessages) return;

    dmMessages.innerHTML = "";
    const messages = JSON.parse(localStorage.getItem(`dm_${userId}`) || "[]");

    messages.forEach(m => {
        const node = document.createElement("div");
        node.innerHTML = `
            <strong>${m.senderName}:</strong> ${m.text}
            <small class="text-muted">${new Date(m.createdAt).toLocaleString()}</small>
        `;
        node.className = m.senderName === currentUserName ? "text-end mb-2" : "text-start mb-2";
        dmMessages.appendChild(node);
    });

    dmMessages.scrollTop = dmMessages.scrollHeight;
}



//
// --- Локальне збереження
//
function appendPrivateMessage(otherId, senderName, text) {
    const key = `dm_${otherId}`;
    const messages = JSON.parse(localStorage.getItem(key) || "[]");

    messages.push({
        senderName,
        text,
        createdAt: new Date().toISOString()
    });

    localStorage.setItem(key, JSON.stringify(messages));
    renderPrivateMessages(otherId);
}



//
// --- SignalR Events
//
connection.on("ReceiveMessage", appendMessage);

connection.on("ReceivePrivateMessage", (m) => {
    const otherId = m.senderId === currentUserId ? m.receiverId : m.senderId;
    const otherName = m.senderId === currentUserId ? "Ви" : m.senderName;

    openDMWindow(otherId, otherName);
    appendPrivateMessage(otherId, m.senderName, m.text);
});

connection.on("UserStatusChanged", (userId, userName, isOnline) => {
    if (isOnline) addOnlineUser(userId, userName);
    else removeOnlineUser(userId);
});

connection.on("InitializeOnlineUsers", (users) => {
    for (const [uid, uname] of Object.entries(users))
        addOnlineUser(uid, uname);
});



//
// --- Надсилання Movie-повідомлень
//
sendBtn?.addEventListener("click", async () => {
    const text = messageInput.value.trim();
    if (!text && !fileInput.files.length) return;

    if (text && movieIdEl) {
        await connection.invoke("SendMessageToMovie", movieIdEl.value, text);
        messageInput.value = '';
    }

    if (fileInput.files.length && movieIdEl) {
        const file = fileInput.files[0];
        const form = new FormData();
        form.append("file", file);

        const resp = await fetch("/Movies/UploadFile", { method: "POST", body: form });
        if (!resp.ok) {
            alert("Upload failed");
            return;
        }

        const json = await resp.json();
        await connection.invoke("SendFileToMovie", movieIdEl.value, json.url, json.fileName);

        fileInput.value = "";
    }
});

messageInput?.addEventListener("keydown", (e) => {
    if (e.key === "Enter") {
        sendBtn.click();
        e.preventDefault();
    }
});



//
// --- Start SignalR
//
async function start() {
    try {
        await connection.start();
        if (movieIdEl) await connection.invoke("JoinMovie", movieIdEl.value);
    } catch (err) {
        console.error(err);
        setTimeout(start, 2000);
    }
}

start();
