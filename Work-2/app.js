// Video player WebApp + simulated AI ThaiPass login
const thaiPassUser = { name: "ThaiPass User (Demo)", id: "THAI-1234" };
console.log("AI: Hello " + thaiPassUser.name + ", welcome to Video App (JS)");

let videos = [
  { title: "Big Buck Bunny", artist: "Blender Foundation • ตัวอย่าง", src: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4", poster: "https://picsum.photos/seed/video1/640/360" },
  { title: "Elephants Dream", artist: "Blender Foundation • ตัวอย่าง", src: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4", poster: "https://picsum.photos/seed/video2/640/360" },
  { title: "For Bigger Blazes", artist: "Google Sample • ตัวอย่าง", src: "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4", poster: "https://picsum.photos/seed/video3/640/360" }
];

const video = document.getElementById("video");
const titleEl = document.getElementById("title");
const artistEl = document.getElementById("artist");
const playBtn = document.getElementById("play");
const prevBtn = document.getElementById("prev");
const nextBtn = document.getElementById("next");
const shufBtn = document.getElementById("shuf");
const repBtn = document.getElementById("rep");
const fullBtn = document.getElementById("full");
const prog = document.getElementById("progress");
const curEl = document.getElementById("cur");
const durEl = document.getElementById("dur");
const volEl = document.getElementById("vol");
const listEl = document.getElementById("list");
const searchEl = document.getElementById("search");
const fileEl = document.getElementById("file");
const countEl = document.getElementById("count");
const aiMsg = document.getElementById("aiMsg");
const aiBtn = document.getElementById("aiBtn");

let idx = 0, isShuffle = false, isRepeat = false, seeking = false;

function aiSay(msg) {
  aiMsg.textContent = "🤖 AI ThaiPass: " + msg;
  console.log("AI ThaiPass (" + thaiPassUser.id + "): " + msg);
}

function fmt(s) {
  if (isNaN(s)) return "0:00";
  s = Math.floor(s);
  return Math.floor(s / 60) + ":" + String(s % 60).padStart(2, "0");
}
function load(i) {
  if (!videos.length) return;
  idx = (i + videos.length) % videos.length;
  const v = videos[idx];
  video.src = v.src;
  video.poster = v.poster || "";
  titleEl.textContent = v.title;
  artistEl.textContent = v.artist;
  render(searchEl.value);
}
function play() { video.play(); playBtn.textContent = "⏸"; }
function pause() { video.pause(); playBtn.textContent = "▶"; }
function toggle() { video.paused ? play() : pause(); }
function next() {
  if (isShuffle) load(Math.floor(Math.random() * videos.length));
  else load(idx + 1);
  play();
  aiSay("กำลังเล่น " + videos[idx].title);
}
function prev() { load(idx - 1); play(); }

function render(q = "") {
  q = q.trim().toLowerCase();
  listEl.innerHTML = "";
  videos.forEach((v, i) => {
    if (q && !v.title.toLowerCase().includes(q) && !v.artist.toLowerCase().includes(q)) return;
    const li = document.createElement("li");
    if (i === idx) li.classList.add("active");
    li.innerHTML = "<img src='" + (v.poster || "https://picsum.photos/seed/video" + i + "/160/90") + "' alt=''>" +
      "<span><b>" + (i === idx ? "▶ " : "🎬 ") + v.title + "</b><br><small>" + v.artist + "</small></span>";
    const x = document.createElement("button");
    x.className = "x"; x.textContent = "✕"; x.title = "Remove video";
    x.onclick = (e) => {
      e.stopPropagation();
      videos.splice(i, 1);
      if (idx >= videos.length) idx = 0;
      if (videos.length) load(idx); else { titleEl.textContent = "No video selected"; video.removeAttribute("src"); video.load(); }
      render(searchEl.value);
    };
    li.appendChild(x);
    li.onclick = () => { load(i); play(); };
    li.ondblclick = () => { load(i); play(); };
    listEl.appendChild(li);
  });
  countEl.textContent = videos.length + " videos";
}

playBtn.onclick = toggle;
nextBtn.onclick = next;
prevBtn.onclick = prev;
shufBtn.onclick = () => { isShuffle = !isShuffle; shufBtn.classList.toggle("on", isShuffle); };
repBtn.onclick = () => { isRepeat = !isRepeat; video.loop = isRepeat; repBtn.classList.toggle("on", isRepeat); };
fullBtn.onclick = () => {
  if (document.fullscreenElement) document.exitFullscreen();
  else if (video.requestFullscreen) video.requestFullscreen();
  else if (video.webkitEnterFullscreen) video.webkitEnterFullscreen();
};
video.onclick = toggle;

aiBtn.onclick = () => {
  if (!videos.length) return;
  const i = Math.floor(Math.random() * videos.length);
  load(i); play();
  aiSay("ขอแนะนำ \"" + videos[i].title + "\" ให้คุณ " + thaiPassUser.name + " 🎉");
};

video.addEventListener("timeupdate", () => {
  if (video.duration && !seeking) prog.value = (video.currentTime / video.duration) * 1000;
  curEl.textContent = fmt(video.currentTime);
  durEl.textContent = fmt(video.duration);
});
prog.addEventListener("pointerdown", () => seeking = true);
prog.addEventListener("pointerup", () => { seeking = false; if (video.duration) video.currentTime = (prog.value / 1000) * video.duration; });
prog.addEventListener("input", () => { if (video.duration && seeking) curEl.textContent = fmt((prog.value / 1000) * video.duration); });
volEl.addEventListener("input", () => { video.volume = volEl.value; video.muted = false; });
video.addEventListener("ended", () => { if (!isRepeat) next(); });
video.addEventListener("play", () => playBtn.textContent = "⏸");
video.addEventListener("pause", () => playBtn.textContent = "▶");
searchEl.addEventListener("input", () => render(searchEl.value));
fileEl.addEventListener("change", (e) => {
  [...e.target.files].forEach(f => {
    videos.push({ title: f.name.replace(/\.[^/.]+$/, ""), artist: "วิดีโอของฉัน", src: URL.createObjectURL(f), poster: "" });
  });
  render(searchEl.value);
  aiSay("เพิ่มวิดีโอใหม่แล้ว " + e.target.files.length + " ไฟล์ ✅");
});

video.volume = 0.8;
load(0);
aiSay("สวัสดี " + thaiPassUser.name + "! เลือกดูวิดีโอได้เลย 👋");
