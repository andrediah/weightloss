# UX & UI Standards

> This document defines UX and UI standards for this project.
> It is intended for use by all designers, developers, and AI coding assistants.
> **AI tools:** Treat these rules as mandatory for all UI generation and front-end code in this project.
> **Developers & Designers:** These standards are enforced in design review and code review.

---

## Core UX Principles

These foundational laws govern all design decisions:

- **Jakob's Law** — Users expect your product to work like other products they already know. Follow established platform conventions before inventing new patterns.
- **Hick's Law** — More choices = more decision time. Reduce options, simplify navigation, and use progressive disclosure to surface secondary content on demand.
- **Fitts's Law** — Interactive elements must be large enough and spaced far enough apart to be easily tapped or clicked without error.
- **Single Responsibility** — Every screen, component, and interaction should do one thing well. Remove any element that does not serve the user's primary goal.
- **Consistency** — Visual, functional, internal, and external consistency must all be maintained. Users should never wonder whether two things that look the same behave the same.

---

## Design System & Component Library

- Maintain a **single component library** as the source of truth for all UI elements
- Document every component with: purpose, usage rules, props/variants, do/don't examples, and accessibility notes
- Use **design tokens** (CSS custom properties / variables) for all colors, spacing, typography, and elevation — never hardcode values
- Version the design system and treat breaking changes with the same care as API changes
- Components must be **independently responsive** — a card or button should adapt to any container, not just the full viewport

### Token naming convention
```css
/* Use semantic names, not descriptive names */
--color-surface-primary         /* Good */
--color-white                   /* Bad — describes appearance, not purpose */

--color-text-primary
--color-text-secondary
--color-text-disabled
--color-border-default
--color-border-focus
--color-feedback-error
--color-feedback-success
--color-feedback-warning
--color-feedback-info

--spacing-xs: 4px
--spacing-sm: 8px
--spacing-md: 16px
--spacing-lg: 24px
--spacing-xl: 32px
--spacing-2xl: 48px

--radius-sm: 4px
--radius-md: 8px
--radius-lg: 16px
--radius-full: 9999px
```

---

## Color System

- Use a **4-layer color model**: Brand, Neutral, Semantic (feedback), and Surface colors
- Define both light and dark mode values for every token from the start
- Never use color as the **only** means of conveying meaning (WCAG 1.4.1) — always pair color with an icon, label, or pattern
- Avoid **fully saturated colors** — they often fail contrast checks and cause eye strain
- Avoid **pure black (#000000)** and **pure white (#FFFFFF)** in interfaces — use near-black and off-white to reduce harshness

### Minimum contrast ratios (WCAG 2.2 Level AA)

| Element | Minimum ratio |
|---|---|
| Normal body text | 4.5:1 |
| Large text (18pt / 14pt bold) | 3:1 |
| UI components and meaningful graphics | 3:1 |
| Decorative elements | No requirement |

> Aim for **Level AAA (7:1)** for body text where possible. Always test every text/background combination — do not assume light-on-dark passes automatically.

---

## Dark Mode

Dark mode is a **required feature**, not an enhancement. It must be implemented from the start, not retrofitted.

### Implementation rules

- Detect the user's OS preference via `prefers-color-scheme` media query and apply it as the **default**
- Always provide a **manual toggle** that persists the user's choice (store in `localStorage` or user profile)
- The toggle must be clearly labeled and easily discoverable — not buried in settings
- Never simply invert light mode colors — design dark mode as its own deliberate palette

### Dark mode color guidelines

- Use **dark grey surfaces** (e.g., `#121212`, `#1E1E1E`) instead of pure black to avoid the halation effect (glowing text edges)
- Use **off-white text** (e.g., `#E8E8E8`, `#FAFAFA`) instead of pure white to reduce eye strain, especially for users with astigmatism
- Reduce color saturation in dark mode — vibrant colors that work on light backgrounds often become harsh on dark ones
- Maintain the full WCAG 4.5:1 contrast ratio for all text — do not assume it automatically passes
- **Focus indicators** must be visible and high-contrast in both modes
- Non-text elements (borders, separators, icons, shadows) must retain visual hierarchy in dark mode — test explicitly

### CSS implementation pattern

```css
:root {
  /* Light mode tokens (default) */
  --color-surface-primary: #FFFFFF;
  --color-surface-secondary: #F5F5F5;
  --color-text-primary: #1A1A1A;       /* 16.6:1 contrast */
  --color-text-secondary: #404040;     /* 10.7:1 contrast */
  --color-text-disabled: #6C6C6C;      /* 4.8:1 contrast */
  --color-border-default: #D4D4D4;
  --color-border-focus: #0066CC;
  --color-feedback-error: #B71C1C;
  --color-feedback-success: #155724;
  --color-feedback-warning: #8A6D00;
}

@media (prefers-color-scheme: dark) {
  :root {
    --color-surface-primary: #121212;
    --color-surface-secondary: #1E1E1E;
    --color-text-primary: #FAFAFA;
    --color-text-secondary: #C7C7C7;
    --color-text-disabled: #757575;
    --color-border-default: #383838;
    --color-border-focus: #60A0FF;
    --color-feedback-error: #EF9A9A;
    --color-feedback-success: #A5D6A7;
    --color-feedback-warning: #FFE082;
  }
}

/* Manual user override */
[data-theme="dark"] { /* same dark token overrides */ }
[data-theme="light"] { /* same light token overrides */ }
```

### Tailwind CSS dark mode configuration

Tailwind v3 and v4 handle class-based dark mode differently. Always declare both to ensure the manual toggle works regardless of CDN version:

```html
<!-- v3: CDN must load first, then set config on the global tailwind object -->
<script src="https://cdn.tailwindcss.com"></script>
<script>tailwind.config = { darkMode: 'class' };</script>

<!-- v4: @custom-variant in a text/tailwindcss style block (ignored by browsers / v3) -->
<style type="text/tailwindcss">
  @custom-variant dark (&:where(.dark, .dark *));
</style>
```

The toggle button must add/remove the `.dark` class on `<html>` and persist the choice to `localStorage`. Use `querySelectorAll('[data-theme-toggle]')` (not a single `getElementById`) so multiple toggle surfaces (e.g. sidebar + mobile header) stay in sync.

---

## Typography

- Use a **maximum of 3 typefaces**: one for headings, one for body, one for accents/code
- Prefer **system fonts** or well-supported web fonts — avoid loading more than 2 custom font families
- Use **variable fonts** where possible for weight/width flexibility with a single file
- All font sizing must use **relative units** (`rem`, `em`, `clamp()`) — never hardcode `px` for font sizes

### Type scale

| Role | Size | Weight | Line height |
|---|---|---|---|
| Display / Hero | `clamp(2rem, 5vw, 3.5rem)` | 700 | 1.1 |
| H1 | `clamp(1.75rem, 4vw, 2.5rem)` | 700 | 1.2 |
| H2 | `clamp(1.5rem, 3vw, 2rem)` | 600 | 1.25 |
| H3 | `clamp(1.25rem, 2.5vw, 1.5rem)` | 600 | 1.3 |
| Body (default) | `1rem` (16px base) | 400 | 1.5 |
| Body small | `0.875rem` | 400 | 1.5 |
| Caption / Label | `0.75rem` (min 12px) | 400 | 1.4 |
| Code / Mono | `0.9rem` | 400 | 1.6 |

### Typography rules

- Set body text at a **minimum of 16px** (1rem) — never go below 12px for any visible text
- Limit line length to **65–80 characters** for body text — prevents eye fatigue on wide screens
- Use **left alignment** for body text — avoid justified text on screen (causes uneven word spacing)
- Use **centered text sparingly** — only for short headings or call-to-action blocks
- Set line height to **1.5× font size** for body text; tighter (1.1–1.25) for large display headings
- Increase letter spacing slightly for **all-caps and small text**
- Use **heading hierarchy sequentially** (H1 → H2 → H3) — never skip levels for visual effect
- Use `CSS clamp()` for fluid typography that scales smoothly between breakpoints without abrupt jumps

---

## Spacing & Layout

- Base all spacing on a **4px or 8px grid** — all margin, padding, and gap values must be multiples of 4px
- Use **CSS custom properties** for all spacing values — never hardcode pixel values in components
- Apply the **Law of Proximity** — group related elements close together; separate unrelated elements with whitespace
- Prefer **CSS Grid** for page-level layouts and **Flexbox** for component-level alignment
- Use **fluid grids** with percentage or `fr` units — never fixed pixel-width columns
- Use **container queries** (`@container`) for component-level responsiveness — not just viewport queries

---

## Responsive Design & Mobile First

- Design **mobile first** — start with the smallest viewport and enhance upward
- Google uses mobile-first indexing — the mobile experience directly affects SEO
- Use **fluid layouts** — avoid fixed widths that require horizontal scrolling
- Define **standard breakpoints**:

```css
/* Mobile first breakpoints */
/* Default: 0px+ (mobile) */
@media (min-width: 640px)  { /* sm: large mobile / small tablet */ }
@media (min-width: 768px)  { /* md: tablet */ }
@media (min-width: 1024px) { /* lg: desktop */ }
@media (min-width: 1280px) { /* xl: large desktop */ }
@media (min-width: 1536px) { /* 2xl: wide screen */ }
```

- Use **container queries** alongside media queries for components that appear in varied contexts (sidebars, modals, grids)
- Use **`srcset` and `sizes`** for all images — never load desktop-resolution images on mobile
- Use **WebP or AVIF** image formats — 30–50% smaller than JPEG/PNG with no visible quality loss
- **Lazy load** images and off-screen content — use `loading="lazy"` on `<img>` tags
- Core tasks must be completable with **touch alone** — test on real devices, not just browser emulators
- Test on a **mid-range Android device** — it represents the majority of global mobile traffic

---

## Navigation

- Navigation must be **consistent** across all pages — position, labels, and behavior must never change unexpectedly
- Use **horizontal menus or mega menus** on desktop; collapse to **hamburger or bottom tab bar** on mobile
- Bottom navigation on mobile places elements within **thumb reach** — preferred over top navigation for primary actions
- Keep navigation labels **short and descriptive** — under 20 characters
- Highlight **active states** clearly so users always know where they are
- All navigation must be **fully keyboard accessible** — visible focus indicators required

### Mobile navigation pattern (≤ 767 px)

- Hide the desktop sidebar entirely (`display: none` / `hidden md:flex`)
- Show a **top header bar** containing the app title/logo and secondary controls (e.g. dark mode toggle)
- Show a **fixed bottom tab bar** with icon + label for each primary destination (max 5 items)
- Bottom tab bar must use `position: fixed; bottom: 0` and account for iOS safe areas: `padding-bottom: env(safe-area-inset-bottom)`
- Main content area must add bottom padding equal to the tab bar height (`pb-24` / 96 px) to prevent content being obscured
- Active tab must be visually distinct — use a top border indicator (`border-top: 2px solid`) in addition to a colour change; never rely on colour alone
- Tab items must meet the 44 × 44 px minimum touch target requirement

---

## Interactive Elements & Forms

### Buttons & touch targets

- Minimum touch target size: **44×44px** (W3C / Apple HIG guideline) — WCAG 2.2 requires 24×24px minimum, but 44×44px is the practical standard
- Provide **adequate spacing** between targets — at least 8px gap to prevent accidental activation
- Use consistent button styles for the same action type across the entire application
- Provide clear **hover, focus, active, and disabled** states for every interactive element

### Forms

- Only ask for information that is **absolutely necessary** — reduce form fields by removing anything non-essential
- Use **inline validation** — show errors on blur (when the user leaves a field), not only on submit
- Place error messages **directly below the relevant field** — never only at the top of the form
- Never use color alone to indicate an error — pair red with an icon and descriptive text
- Use **descriptive placeholder text** only as a hint, never as a label — labels must always be visible
- Group related fields visually with spacing and/or fieldsets
- For long forms, show a **progress indicator** and allow users to save their progress

### Error messages

- Write errors in **plain language** — explain what went wrong and how to fix it
- Never use technical jargon or error codes as the primary message
- Good pattern: `"Email address is required. Please enter your email to continue."`
- Bad pattern: `"Error 422: Validation failed"`

---

## Feedback & System Status

- Always communicate **system status** — users must know if something is loading, saving, or has failed (Nielsen Heuristic #1)
- Loading states: use **skeleton screens** for content areas; use **spinners** only for brief indeterminate waits
- Use **micro-interactions** to confirm actions — a button ripple, a save checkmark, a subtle animation — these signal responsiveness
- Provide **confirmation** for irreversible or high-stakes actions (delete, submit, purchase) — use a modal or inline confirmation
- Feedback response time: aim for **< 100ms** for instant feel; < 400ms for perceived responsiveness; > 1s requires a visible indicator

---

## Accessibility (WCAG 2.2 Level AA — Required)

All interfaces must meet **WCAG 2.2 Level AA** as a minimum. Level AAA is the target for text contrast.

### Perceivable

- All images must have **descriptive `alt` text** — decorative images use `alt=""`
- Never use **color alone** to convey meaning — always pair with text, icon, or pattern
- All media (video, audio) must have **captions or transcripts**
- Content must reflow at 400% zoom without horizontal scrolling (WCAG 1.4.10)

### Operable

- All functionality must be accessible via **keyboard alone** — no mouse required
- Visible **focus indicators** are required on all interactive elements in both light and dark mode
- No content should **flash more than 3 times per second** (seizure risk)
- Minimum touch target: **24×24px** (WCAG 2.5.8); recommended **44×44px**
- Provide **skip navigation links** for screen reader and keyboard users

### Understandable

- Use **semantic HTML** — headings, lists, buttons, and form labels must use correct elements
- Form inputs must have **associated `<label>` elements** — never use placeholder as a substitute
- Error messages must be **programmatically associated** with their input (`aria-describedby`)
- Language of page must be set (`<html lang="en">`)

### Robust

- All UI must be compatible with **current assistive technologies** (NVDA, JAWS, VoiceOver)
- Use **ARIA attributes** only when native HTML semantics are insufficient — prefer native elements
- Test with a **screen reader** before shipping any new component or page

### Testing tools

- **axe DevTools** — automated accessibility scanning
- **Lighthouse** — built into Chrome DevTools; run on every page
- **Stark** (Figma plugin) — contrast and color blindness simulation
- **WebAIM Contrast Checker** — manual contrast verification
- **NVDA / VoiceOver** — real screen reader testing

---

## Performance as UX

- Target a **Largest Contentful Paint (LCP) < 2.5s** and **Interaction to Next Paint (INP) < 200ms**
- Establish **performance budgets** — set limits for page weight, load times, and interaction delays
- Compress all assets — use **Gzip or Brotli** for text assets, **WebP/AVIF** for images
- **Lazy load** all off-screen images and non-critical components
- Consider **low-bandwidth scenarios** — a beautiful design that takes 10 seconds to load is not good UX
- Use **server-side rendering (SSR)** or **static generation** for content-heavy pages to improve perceived speed
- Never block the main thread with heavy JavaScript on initial load

---

## UX Writing & Tone of Voice

- Write in **plain language** — aim for a Grade 8 reading level for general audiences
- Use **active voice** — "Save your changes" not "Changes will be saved"
- Be **specific and actionable** — "Enter your 6-digit code" not "Enter the code"
- Never use vague error messages — always tell the user what happened and what to do next
- Use **sentence case** for UI labels and buttons — not Title Case or ALL CAPS (exceptions: acronyms)
- Keep button labels to **1–3 words** that clearly describe the action: "Save", "Delete account", "Send message"
- Avoid **jargon and technical terms** in user-facing copy unless the audience is explicitly technical
- Empty states must be **helpful** — explain why the space is empty and what the user can do

---

## Iconography

- Use **universally recognized icons** for common actions (search, menu, settings, close, back)
- Always use **SVG format** — scalable, accessible, and themeable
- Icons must have a **text label or `aria-label`** — never rely on icon alone for meaning
- Maintain **stylistic consistency** across all icons — use a single icon library or family
- Icon minimum size: **24×24px** for interactive icons; **16×16px** for inline/decorative

---

## Motion & Animation

- All animations must **respect `prefers-reduced-motion`** — disable or reduce motion for users who have set this preference

```css
@media (prefers-reduced-motion: reduce) {
  *, *::before, *::after {
    animation-duration: 0.01ms !important;
    transition-duration: 0.01ms !important;
  }
}
```

- Use animation **purposefully** — to guide attention, confirm actions, or show relationships — not for decoration
- Keep transitions **fast**: 150–300ms for micro-interactions; 300–500ms for layout transitions
- Avoid **auto-playing video or animation** that cannot be paused
- Never use content that **flashes more than 3 times per second** — it is a seizure risk and a WCAG violation

---

## Privacy & Trust UX

- Provide users with a **clear and honest** cookie/data consent experience — do not use dark patterns
- Default cookie preferences to the **most privacy-preserving option**
- Do not use **pre-checked consent boxes** for optional data collection
- Make it **as easy to opt out** as it is to opt in
- Data collection dialogs must clearly state **what is collected and why**, in plain language
- Provide users with visibility into and control over their own data

---

## Using This File With AI Coding Assistants

| Tool | What to do |
|---|---|
| **Claude Code** | Rename or symlink to `CLAUDE.md` in project root |
| **Cursor** | Rename or symlink to `.cursorrules` in project root |
| **GitHub Copilot** | Copy to `.github/copilot-instructions.md` |
| **Windsurf** | Rename or symlink to `.windsurfrules` in project root |
| **Aider** | Pass via `--system-prompt ux-standards.md` |
| **ChatGPT / Gemini** | Paste contents into the system prompt or custom instructions |

> **Tip:** Keep this as `ux-standards.md` and symlink per tool so all stay in sync:
> ```bash
> ln -s ux-standards.md .cursorrules
> ln -s ux-standards.md .windsurfrules
> ```

---

## Related Standards Files

| File | Purpose |
|---|---|
| `coding-standards.md` | C# / .NET backend and API rules |
| `ux-standards.md` | This file — UI, UX, accessibility, dark mode |
| `api-standards.md` | REST API design conventions (optional) |
| `testing-standards.md` | Testing philosophy and tooling (optional) |
