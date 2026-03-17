import {ActivatedRouteSnapshot, CanActivateFn, RedirectCommand, RouterStateSnapshot} from "@angular/router";
import {AuthService} from "./auth.service";
import {inject} from "@angular/core";
import {Router} from "@angular/router";

export const AuthGuard: CanActivateFn = (
    _route: ActivatedRouteSnapshot,
    _state: RouterStateSnapshot,
) => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
        const loginPath = router.parseUrl("/login");
        return new RedirectCommand(loginPath, {
            skipLocationChange: true,
        });
    }

    return true ;

    //TODO: Return "RedirectCommand" to login screen
}