import { ThemeService } from './theme.service';

export function themeAppInitializer(theme: ThemeService): () => void {
  return () => {
    theme.initFromStorage();
  };
}
