import { Injectable } from '@angular/core';

const DEFAULT_HEX = '#1a3a5c';

function normalizeHex(input: string | null | undefined): string | null {
  if (!input?.trim()) return null;
  let s = input.trim();
  if (!s.startsWith('#')) s = `#${s}`;
  if (/^#[0-9a-fA-F]{6}$/.test(s)) return s.toLowerCase();
  if (/^#[0-9a-fA-F]{3}$/.test(s)) {
    const r = s[1];
    const g = s[2];
    const b = s[3];
    return (`#${r}${r}${g}${g}${b}${b}`).toLowerCase();
  }
  return null;
}

function hexToRgb(hex: string): { r: number; g: number; b: number } | null {
  const h = normalizeHex(hex);
  if (!h) return null;
  return {
    r: parseInt(h.slice(1, 3), 16),
    g: parseInt(h.slice(3, 5), 16),
    b: parseInt(h.slice(5, 7), 16),
  };
}

function mixBlack(rgb: { r: number; g: number; b: number }, factor: number): string {
  const r = Math.round(rgb.r * factor);
  const g = Math.round(rgb.g * factor);
  const b = Math.round(rgb.b * factor);
  return `${r}, ${g}, ${b}`;
}

/** Primario mezclado con casi-negro (sidebar oscura); `w` = peso del primario, bajo = casi negro. */
function blendPrimaryNearBlack(rgb: { r: number; g: number; b: number }, primaryWeight: number): string {
  const br = 5;
  const bg = 9;
  const bb = 16;
  const t = Math.max(0, Math.min(1, primaryWeight));
  const r = Math.round(rgb.r * t + br * (1 - t));
  const g = Math.round(rgb.g * t + bg * (1 - t));
  const b = Math.round(rgb.b * t + bb * (1 - t));
  return `${r}, ${g}, ${b}`;
}

function lightenRgb(rgb: { r: number; g: number; b: number }, amount: number): string {
  const r = Math.round(rgb.r + (255 - rgb.r) * amount);
  const g = Math.round(rgb.g + (255 - rgb.g) * amount);
  const b = Math.round(rgb.b + (255 - rgb.b) * amount);
  return `rgb(${r}, ${g}, ${b})`;
}

@Injectable({ providedIn: 'root' })
export class BrandThemeService {
  /** Aplica color de compañía en variables CSS (Bootstrap + sidebar). */
  applyFromHex(hex: string | null | undefined): void {
    const h = normalizeHex(hex) ?? DEFAULT_HEX;
    const rgb = hexToRgb(h);
    if (!rgb) return;

    const root = document.documentElement;
    root.style.setProperty('--bs-primary', h);
    root.style.setProperty('--bs-primary-rgb', `${rgb.r}, ${rgb.g}, ${rgb.b}`);
    root.style.setProperty('--gest-navy-950', `rgb(${mixBlack(rgb, 0.42)})`);
    root.style.setProperty('--gest-navy-900', `rgb(${mixBlack(rgb, 0.55)})`);
    root.style.setProperty('--gest-navy-800', h);
    root.style.setProperty('--gest-navy-700', lightenRgb(rgb, 0.14));
    root.style.setProperty('--bs-link-color', h);

    // Sidebar tema oscuro: casi negro con ligero matiz del primario (primarios saturados quedaban muy fuertes con solo mixBlack).
    root.style.setProperty('--gest-sidebar-dark-start', `rgb(${blendPrimaryNearBlack(rgb, 0.11)})`);
    root.style.setProperty('--gest-sidebar-dark-end', `rgb(${blendPrimaryNearBlack(rgb, 0.17)})`);
  }

  /** Quita sobrescrituras; vuelve a los valores del CSS compilado. */
  resetBrand(): void {
    const keys = [
      '--bs-primary',
      '--bs-primary-rgb',
      '--gest-navy-950',
      '--gest-navy-900',
      '--gest-navy-800',
      '--gest-navy-700',
      '--gest-sidebar-dark-start',
      '--gest-sidebar-dark-end',
      '--bs-link-color',
    ];
    const root = document.documentElement;
    for (const k of keys) root.style.removeProperty(k);
  }
}
