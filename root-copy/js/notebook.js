// llote docs -- notebook page-flip controller.
//
// Replaces Canary's default hybrid-router content swap (see runtime/ts/
// hybrid-router.ts in the Canary repo) with a bespoke spiral-notebook flip:
// every page in PAGES is fetched once up front and built into a stacked
// deck of .page elements inside #notebook, then all navigation (prev/next
// controls, keyboard arrows, browser back/forward) just toggles which
// pages are "flipped" -- no further fetching, no per-click network request.
//
// registerRouteHandler/start come straight from Canary's own router.js
// (pushState + click interception + popstate), so real per-route URLs and
// browser history keep working exactly like any other Canary site -- only
// the RENDERING of a route change is different here.
import { registerRouteHandler, start } from "/js/router.js";

const PAGES = [
  { path: "", title: "Cover" },
  { path: "install", title: "Install" },
  { path: "quickstart", title: "Quick Start" },
  { path: "commands", title: "Commands" },
  { path: "file-format", title: "File Format" },
  { path: "philosophy", title: "Philosophy & FAQ" },
];

function normalize(path) {
  return path.replace(/^\/+|\/+$/g, "");
}

function hrefFor(path) {
  return path === "" ? "/" : `/${path}/`;
}

function indexForPath(path) {
  const n = normalize(path);
  const i = PAGES.findIndex((p) => p.path === n);
  return i === -1 ? 0 : i;
}

const notebook = document.getElementById("notebook");
const appEl = document.getElementById("app");
const prevLink = document.getElementById("nb-prev");
const nextLink = document.getElementById("nb-next");
const indicator = document.getElementById("nb-indicator");

let current = indexForPath(window.location.pathname);
let pageEls = [];
let ready = false;

function buildPageEl() {
  const page = document.createElement("section");
  page.className = "page";
  page.innerHTML = `
    <div class="face face-front">
      <div class="page-content"></div>
      <span class="shadow" aria-hidden="true"></span>
    </div>
    <div class="face face-back">
      <span class="shadow" aria-hidden="true"></span>
    </div>
  `;
  notebook.insertBefore(page, appEl);
  return page;
}

async function fetchContent(path) {
  if (path === normalize(window.location.pathname)) {
    return appEl.innerHTML;
  }
  try {
    const res = await fetch(hrefFor(path));
    if (!res.ok) return "<p>Not found.</p>";
    const html = await res.text();
    const fragment = new DOMParser().parseFromString(html, "text/html").getElementById("app");
    return fragment ? fragment.innerHTML : "<p>Could not load this page.</p>";
  } catch {
    return "<p>Could not load this page.</p>";
  }
}

function applyStacking() {
  const total = PAGES.length;
  pageEls.forEach((el, i) => {
    el.style.zIndex = i < current ? total + i : total - i;
  });
}

function updateControls() {
  const total = PAGES.length;
  const atStart = current <= 0;
  const atEnd = current >= total - 1;

  prevLink.href = hrefFor(PAGES[atStart ? 0 : current - 1].path);
  prevLink.toggleAttribute("aria-disabled", atStart);
  prevLink.tabIndex = atStart ? -1 : 0;

  nextLink.href = hrefFor(PAGES[atEnd ? total - 1 : current + 1].path);
  nextLink.toggleAttribute("aria-disabled", atEnd);
  nextLink.tabIndex = atEnd ? -1 : 0;

  indicator.textContent = `${current + 1} / ${total} — ${PAGES[current].title}`;
  document.title = current === 0 ? "llote" : `llote — ${PAGES[current].title}`;
}

function applyState(animate) {
  pageEls.forEach((el, i) => {
    const flipped = i < current;
    if (!animate) {
      el.classList.add("no-anim");
    }
    el.classList.add("turning");
    el.classList.toggle("flipped", flipped);
    if (!animate) {
      void el.offsetWidth; // force reflow so the no-anim transition-less state actually applies
      el.classList.remove("no-anim");
      el.classList.remove("turning");
    } else {
      el.addEventListener("transitionend", () => el.classList.remove("turning"), { once: true });
    }
  });
  applyStacking();
  updateControls();
}

async function init() {
  pageEls = PAGES.map(() => buildPageEl());
  const contents = await Promise.all(PAGES.map((p) => fetchContent(p.path)));
  contents.forEach((html, i) => {
    pageEls[i].querySelector(".page-content").innerHTML = html;
  });
  appEl.style.display = "none";
  ready = true;
  applyState(false);
}

registerRouteHandler((route) => {
  const idx = indexForPath(route.path);
  if (!ready) {
    current = idx;
    return;
  }
  const animate = Math.abs(idx - current) === 1;
  current = idx;
  applyState(animate);
});

// prefers-reduced-motion is handled purely in CSS (see theme.css) --
// the transition duration collapses to ~0 there, so this JS doesn't need
// to branch on it at all.
document.addEventListener("keydown", (e) => {
  if (e.metaKey || e.ctrlKey || e.altKey) return;
  if (e.key === "ArrowRight" && !nextLink.hasAttribute("aria-disabled")) {
    e.preventDefault();
    nextLink.click();
  } else if (e.key === "ArrowLeft" && !prevLink.hasAttribute("aria-disabled")) {
    e.preventDefault();
    prevLink.click();
  }
});

init();
start();
