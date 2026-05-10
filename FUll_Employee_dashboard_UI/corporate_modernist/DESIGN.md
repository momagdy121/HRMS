---
name: Corporate Modernist
colors:
  surface: '#f9f9fd'
  surface-dim: '#d9dade'
  surface-bright: '#f9f9fd'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f3f3f7'
  surface-container: '#ededf2'
  surface-container-high: '#e8e8ec'
  surface-container-highest: '#e2e2e6'
  on-surface: '#1a1c1f'
  on-surface-variant: '#42474f'
  inverse-surface: '#2f3034'
  inverse-on-surface: '#f0f0f4'
  outline: '#72777f'
  outline-variant: '#c2c7d0'
  surface-tint: '#35618d'
  primary: '#00375e'
  on-primary: '#ffffff'
  primary-container: '#1f4e79'
  on-primary-container: '#95bff1'
  inverse-primary: '#a0cafc'
  secondary: '#505f76'
  on-secondary: '#ffffff'
  secondary-container: '#d0e1fb'
  on-secondary-container: '#54647a'
  tertiary: '#323537'
  on-tertiary: '#ffffff'
  tertiary-container: '#494c4e'
  on-tertiary-container: '#babcbe'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d1e4ff'
  primary-fixed-dim: '#a0cafc'
  on-primary-fixed: '#001d35'
  on-primary-fixed-variant: '#184974'
  secondary-fixed: '#d3e4fe'
  secondary-fixed-dim: '#b7c8e1'
  on-secondary-fixed: '#0b1c30'
  on-secondary-fixed-variant: '#38485d'
  tertiary-fixed: '#e0e3e5'
  tertiary-fixed-dim: '#c4c7c9'
  on-tertiary-fixed: '#191c1e'
  on-tertiary-fixed-variant: '#444749'
  background: '#f9f9fd'
  on-background: '#1a1c1f'
  surface-variant: '#e2e2e6'
typography:
  h1:
    fontFamily: Inter
    fontSize: 30px
    fontWeight: '700'
    lineHeight: 38px
    letterSpacing: -0.02em
  h2:
    fontFamily: Inter
    fontSize: 24px
    fontWeight: '600'
    lineHeight: 32px
    letterSpacing: -0.01em
  h3:
    fontFamily: Inter
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
  body-sm:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '400'
    lineHeight: 18px
  label-bold:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: 16px
  badge:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '500'
    lineHeight: 12px
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  base: 4px
  xs: 4px
  sm: 8px
  md: 16px
  lg: 24px
  xl: 32px
  sidebar-width: 260px
  sidebar-collapsed: 72px
  navbar-height: 64px
---

## Brand & Style

This design system is built upon a **Corporate Modernist** aesthetic, prioritizing institutional trust, efficiency, and clarity. The target audience—HR professionals and corporate employees—requires an environment that feels stable and systematic. 

The visual language balances the authoritative weight of deep navy tones with the openness of expansive light grey workspaces. Minimalism is employed to reduce cognitive load during data-intensive tasks, while subtle depth markers ensure the interface remains intuitive and layered. The emotional response is one of organized reliability and professional calm.

## Colors

The palette is anchored by the primary **#1F4E79** (Deep Navy), used strategically for the sidebar and primary actions to establish a strong structural frame. Backgrounds utilize a cool light grey to prevent screen glare and distinguish content areas from the workspace.

- **Primary:** Used for structural elements and high-priority interactions.
- **Secondary:** A muted blue-grey for secondary icons and supporting text.
- **Surface:** Pure white for cards and input areas to ensure maximum legibility.
- **Functional:** A semantic palette for status badges, utilizing high-chroma tones for immediate recognition against the neutral background.

## Typography

This design system utilizes **Inter** exclusively to leverage its exceptional readability and systematic weights. The typographic hierarchy is strictly enforced to guide users through complex HR forms and data tables. 

Headline levels use slight negative letter-spacing for a more compact, modern feel. Body text defaults to 14px for standard data density, while 12px uppercase labels are reserved for table headers and section descriptors to create clear visual separation without adding weight.

## Layout & Spacing

The design system employs a **fixed-fluid hybrid grid**. The left sidebar is fixed at 260px, collapsing to a 72px icon-only rail or a hamburger menu on mobile. The top navbar is fixed to the viewport, ensuring global navigation and user profile access are always available.

Main content resides in a fluid container with a standard 32px padding on desktop. Data tables and form groups use an 8px (base-2) spacing rhythm to maintain alignment. Information density is prioritized, using "comfortable" vertical spacing for lists and "compact" spacing for data grids.

## Elevation & Depth

Hierarchy is established through **ambient shadows** and tonal layering rather than high-contrast borders.

- **Level 0 (Floor):** The #F1F5F9 main background.
- **Level 1 (Cards):** Crisp white surfaces with a subtle 4px blur, 2px Y-offset shadow at 5% opacity. This identifies interactive zones.
- **Level 2 (Toasts/Modals):** Higher elevation with a 12px blur and 10% opacity shadow to indicate temporary overlay status.
- **Sidebar Depth:** The dark #1F4E79 sidebar uses no shadow but relies on its color value to create a perceived "underlay" beneath the main workspace.

## Shapes

The shape language is **Soft (Level 1)**, utilizing a 4px (0.25rem) radius for standard elements like input fields, buttons, and badges. This subtle rounding maintains the professional "geometric" feel of a corporate system while softening the harshness of a pure 0px edge. Large containers like cards may utilize the `rounded-lg` (8px) token to further emphasize their role as primary content buckets.

## Components

### Sidebar & Navigation
The sidebar uses #1F4E79 as the base. Active states should be indicated by a solid left-border accent (4px) in a lighter blue or a subtle background highlight. Top navbar should be white with a faint bottom border (#E2E8F0).

### Data Tables
Tables are "striped," with even rows utilizing a faint #F8FAFC tint. Headers use `label-bold` typography with #64748B text color. Hover states on rows are mandatory for row tracking.

### Status Badges
Badges use a "soft pill" style: a low-opacity background of the status color with high-contrast text of the same hue.
- **Approved:** Light green bg / Dark green text.
- **Pending:** Light yellow bg / Amber text.

### Buttons
Primary buttons use the solid #1F4E79 background with white text. Secondary buttons use a ghost style (outline) or a light grey fill.

### Toast Notifications
Positioned top-right, these should have a 1px border matching the status color (e.g., green for success) and a white card background to ensure they "pop" against the UI.