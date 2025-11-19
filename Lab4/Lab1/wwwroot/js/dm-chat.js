// dm-chat.js — vanilla JS DM manager (localStorage full-history)
"use strict";

const hubUrl = "/movieChatHub";
let connection = null;
let currentUserId = null; // буде ініціалізовано при старті
const DM_PREFIX = "dm_history:"; // localStorage key prefix

function storageKeyFor(userId) { return `${DM_PREFIX}${userId}`; }

// --- load / save history ---
function loadHistory() {
    const key = storageKeyFor(currentUserId);
    const txt = localStorage.getItem(key);
    if (!txt) return { conversations: {}, meta: { lastUpdated: new Date().toISOString() } };
    try { return JSON.parse(txt); } catch (e) { return { conversations: {}, meta: { lastUpdated: new Date().toISOString() } }; }
}

function saveHistory(hist) {
    const key = storageKeyFor(currentUserId);
    hist.meta = hist.meta || {};
    hist.meta.lastUpdated = new Date().toISOString();
    localStorage.setItem(key, JSON.stringify(hist));
}

function pushMessage(otherUserId, direction, text, fileUrl = null, fileName = null, createdAt = null) {
    const hist = loadHistory();
    hist.conversations = hist.conversations || {};
    hist.conversations[otherUserId] = hist.conversations[otherUserId] || [];
    hist.conversations[otherUserId].push({
        id: Math.floor(Math.random() * 1e9),
        from: (direction === "me" ? "me" : "them"),
        text: text || null,
        fileUrl: fileUrl || null,
        fileName: fileName || null,
        createdAt: createdAt || new Date().toISOString()
    });
    saveHistory(hist);
}

// --- escape helper ---
function escapeHtml(s) { if (!s) return ""; return s.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;"); }

// --- render conversation into DM window ---
function renderConversation(otherUserId) {
    const conv = loadHistory().conversations?.[otherUserId] || [];
    const box = document.getElementById(`dmMessages-${otherUserId}`);
    if (!box) return;
    box.innerHTML = "";
    conv.forEach(m => {
        const el = document.createElement("div");
        el.className = m.from === "me" ? "text-end mb-2" : "text-start mb-2";
        let inner = "";
        if (m.text) inner += `<div class="p-2 ${m.from === 'me' ? 'bg-primary text-white rounded' : 'bg-light rounded'}">${escapeHtml(m.text)}</div>`;
        if (m.fileUrl) inner += `<div class="mt-1"><a href="${m.fileUrl}" target="_blank">${escapeHtml(m.fileName || 'file')}</a></div>`;
        inner += `<div class="small text-muted">${new Date(m.createdAt).toLocaleString()}</div>`;
        el.innerHTML = inner;
        box.appendChild(el);
    });
    box.scrollTop = box.scrollHeight;
}

// --- open DM window ---
function openDMWindow(otherUserId, otherUserName) {
    let wrapper = document.getElementById(`dm-${otherUserId}`);
    if (!wrapper) {
        wrapper = document.createElement("div");
        wrapper.id = `dm-${otherUserId}`;
        wrapper.className = "dm-window border p-2 bg-light position-fixed";
        wrapper.style.bottom = "10px";
        wrapper.style.right = `${10 + document.querySelectorAll('.dm-window').length * 240}px`;
        wrapper.style.width = "220px";
        wrapper.style.boxShadow = "0 2px 8px rgba(0,0,0,0.2)";
        wrapper.style.cursor = "move";
        wrapper.innerHTML = `
            <div class="d-flex justify-content-between align-items-center mb-1" id="dmHeader-${otherUserId}">
                <strong>${otherUserName}</strong>
                <div>
                    <button class="btn btn-sm btn-light" id="toggleDM-${otherUserId}">🗕</button>
                    <button class="btn btn-sm btn-light" id="closeDM-${otherUserId}">✖</button>
                </div>
            </div>
            <div class="dm-body" id="dmBody-${otherUserId}">
                <div class="dm-messages mb-1" id="dmMessages-${otherUserId}" style="height:150px; overflow:auto; background:#fff; padding:5px; border:1px solid #ccc;"></div>
                <input type="text" class="form-control form-control-sm mb-1" placeholder="Написати..." id="dmInput-${otherUserId}" />
                <button class="btn btn-sm btn-primary w-100" id="dmSend-${otherUserId}">Відправити</button>
            </div>
        `;
        document.body.appendChild(wrapper);

        // draggable
        const header = document.getElementById(`dmHeader-${otherUserId}`);
        let isDragging = false, startX, startY, startLeft, startTop;
        header.addEventListener('mousedown', (e) => {
            isDragging = true;
            startX = e.clientX;
            startY = e.clientY;
            startLeft = parseInt(wrapper.style.right || 0);
            startTop = parseInt(wrapper.style.bottom || 0);
            document.body.style.userSelect = 'none';
        });
        document.addEventListener('mousemove', (e) => {
            if (!isDragging) return;
            const dx = startX - e.clientX;
            const dy = startY - e.clientY;
            wrapper.style.right = `${startLeft + dx}px`;
            wrapper.style.bottom = `${startTop + dy}px`;
        });
        document.addEventListener('mouseup', () => { isDragging = false; document.body.style.userSelect = 'auto'; });

        // close DM
        document.getElementById(`closeDM-${otherUserId}`).addEventListener("click", () => wrapper.remove());

        // toggle DM
        const toggleBtn = document.getElementById(`toggleDM-${otherUserId}`);
        const dmBody = document.getElementById(`dmBody-${otherUserId}`);
        toggleBtn.addEventListener("click", () => {
            if (dmBody.style.display === "none") {
                dmBody.style.display = "block";
                toggleBtn.textContent = "🗕";
            } else {
                dmBody.style.display = "none";
                toggleBtn.textContent = "🗖";
            }
        });

        // send DM
        const sendBtn = document.getElementById(`dmSend-${otherUserId}`);
        const inputEl = document.getElementById(`dmInput-${otherUserId}`);
        sendBtn.addEventListener("click", () => {
            const text = inputEl.value.trim();
            if (!text) return;
            pushMessage(otherUserId, "me", text);
            renderConversation(otherUserId);
            inputEl.value = "";
            if (connection) connection.invoke("SendPrivateMessage", otherUserId, text).catch(console.error);
        });

        // send on Enter
        inputEl.addEventListener("keydown", (e) => {
            if (e.key === "Enter") { sendBtn.click(); e.preventDefault(); }
        });
    }

    renderConversation(otherUserId);
}

// --- typing indicator ---
window.privateTyping = async (receiverUserId, inputEl) => {
    if (!connection) return;
    connection.invoke("TypingPrivate", receiverUserId).catch(() => { });
    clearTimeout(window._privateTypingTimer);
    window._privateTypingTimer = setTimeout(() => {
        connection.invoke("StopTypingPrivate", receiverUserId).catch(() => { });
    }, 1500);
};

// --- SignalR connection ---
async function startDmHub(userId) {
    currentUserId = userId;
    connection = new signalR.HubConnectionBuilder().withUrl(hubUrl).withAutomaticReconnect().build();

    connection.on("ReceivePrivateMessage", (msg) => {
        const otherId = msg.senderId === currentUserId ? msg.receiverId : msg.senderId;
        const from = msg.senderId === currentUserId ? "me" : "them";
        pushMessage(msg.senderId, from, msg.text, msg.fileUrl || null, msg.fileName || null, msg.createdAt || new Date().toISOString());
        openDMWindow(otherId, msg.senderName || otherId);
        renderConversation(otherId);
    });

    connection.on("UserTyping", (whoId, whoName) => {
        const el = document.getElementById(`dmTyping-${whoId}`);
        if (el) { el.style.display = "block"; clearTimeout(el._t); el._t = setTimeout(() => el.style.display = "none", 2000); }
    });

    connection.on("UserStatusChanged", (uid, isOnline) => {
        const st = document.getElementById(`dm-status-${uid}`);
        if (st) { st.textContent = isOnline ? "online" : "offline"; st.className = isOnline ? "text-success small" : "text-secondary small"; }
    });

    try {
        await connection.start();
        console.log("DM hub connected");
    } catch (err) {
        console.error(err);
        setTimeout(() => startDmHub(userId), 2000);
    }
}

// --- expose methods globally ---
window.DM = {
    start: startDmHub,
    open: openDMWindow,
    loadHistoryForCurrent: () => loadHistory()
};
