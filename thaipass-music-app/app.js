// Music player app in JS + simulated AI ThaiPass login
const thaiPassUser = { name: "ThaiPass User (Demo)", id: "THAI-1234" };
console.log("AI: Hello " + thaiPassUser.name + ", welcome to Music App (JS)");

let songs = [
  { title: "SoundHelix Song 1", artist: "T. Schürger", src: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-1.mp3", cover: "https://picsum.photos/seed/music1/300/300" },
  { title: "SoundHelix Song 2", artist: "T. Schürger", src: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-2.mp3", cover: "https://picsum.photos/seed/music2/300/300" },
  { title: "SoundHelix Song 3", artist: "T. Schürger", src: "https://www.soundhelix.com/examples/mp3/SoundHelix-Song-3.mp3", cover: "https://picsum.photos/seed/music3/300/300" }
];

const audio = document.getElementById("audio");
const titleEl = document.getElementById("title");
const artistEl = document.getElementById("artist");
const coverEl = document.getElementById("cover");
const playBtn = document.getElementById("play");
const prevBtn = document.getElementById("prev");
const nextBtn = document.getElementById("next");
const shufBtn = document.getElementById("shuf");
const repBtn = document.getElementById("rep");
const prog = document.getElementById("progress");
const curEl = document.getElementById("cur");
const durEl = document.getElementById("dur");
const volEl = document.getElementById("vol");
const listEl = document.getElementById("list");
const searchEl = document.getElementById("search");
const fileEl = document.getElementById("file");
const countEl = document.getElementById("count");

let idx = 0, isShuffle = false, isRepeat = false, seeking = false;

function fmt(s) {
  if (isNaN(s)) return "0:00";
  s = Math.floor(s);
  return Math.floor(s / 60) + ":" + String(s % 60).padStart(2, "0");
}
function load(i) {
  idx = (i + songs.length) % songs.length;
  const s = songs[idx];
  audio.src = s.src;
  titleEl.textContent = s.title;
  artistEl.textContent = s.artist;
  coverEl.src = s.cover;
  render(searchEl.value);
}
function play() { audio.play(); playBtn.textContent = "⏸"; }
function pause() { audio.pause(); playBtn.textContent = "▶"; }
function toggle() { audio.paused ? play() : pause(); }
function next() {
  if (isShuffle) load(Math.floor(Math.random() * songs.length));
  else load(idx + 1);
  play();
}
function prev() { load(idx - 1); play(); }

function render(q = "") {
  q = q.trim().toLowerCase();
  listEl.innerHTML = "";
  songs.forEach((s, i) => {
    if (q && !s.title.toLowerCase().includes(q) && !s.artist.toLowerCase().includes(q)) return;
    const li = document.createElement("li");
    if (i === idx) li.classList.add("active");
    li.innerHTML = "<span><b>" + (i === idx ? "▶ " : "🎵 ") + s.title + "</b><br><small>" + s.artist + "</small></span>";
    const x = document.createElement("button");
    x.className = "x"; x.textContent = "✕"; x.title = "Remove song";
    x.onclick = (e) => {
      e.stopPropagation();
      songs.splice(i, 1);
      if (idx >= songs.length) idx = 0;
      if (songs.length) load(idx); else { titleEl.textContent = "No song selected"; audio.src = ""; }
      render(searchEl.value);
    };
    li.appendChild(x);
    li.onclick = () => { load(i); play(); };
    listEl.appendChild(li);
  });
  countEl.textContent = songs.length + " songs";
}

playBtn.onclick = toggle;
nextBtn.onclick = next;
prevBtn.onclick = prev;
shufBtn.onclick = () => { isShuffle = !isShuffle; shufBtn.classList.toggle("on", isShuffle); };
repBtn.onclick = () => { isRepeat = !isRepeat; audio.loop = isRepeat; repBtn.classList.toggle("on", isRepeat); };

audio.addEventListener("timeupdate", () => {
  if (audio.duration && !seeking) prog.value = (audio.currentTime / audio.duration) * 1000;
  curEl.textContent = fmt(audio.currentTime);
  durEl.textContent = fmt(audio.duration);
});
prog.addEventListener("pointerdown", () => seeking = true);
prog.addEventListener("pointerup", () => { seeking = false; if (audio.duration) audio.currentTime = (prog.value / 1000) * audio.duration; });
prog.addEventListener("input", () => { if (audio.duration && seeking) { curEl.textContent = fmt((prog.value / 1000) * audio.duration); } });
volEl.addEventListener("input", () => audio.volume = volEl.value);
audio.addEventListener("ended", () => { if (!isRepeat) next(); });
searchEl.addEventListener("input", () => render(searchEl.value));
fileEl.addEventListener("change", (e) => {
  [...e.target.files].forEach(f => {
    songs.push({ title: f.name.replace(/\.[^/.]+$/, ""), artist: "My Song", src: URL.createObjectURL(f), cover: "https://picsum.photos/seed/" + Date.now() + Math.random() + "/300/300" });
  });
  render(searchEl.value);
  alert("Song(s) added!");
});

audio.volume = 0.8;
load(0);
