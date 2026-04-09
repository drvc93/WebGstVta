import { AuthService } from './auth.service';

export function brandAppInitializer(auth: AuthService): () => void {
  return () => {
    auth.hydrateFromStorage();
  };
}
