import { JwtPayload, jwtDecode } from "jwt-decode";

export abstract class Role {
  static ALLOW_CREATE_TAP: string = "MyAccount.VNext.CreateTAP";
  static ALLOW_DISMISS_USER_RISK: string = "MyAccount.VNext.DismissUserRisk";
  static ALLOW_CHANGE_PASSWORD: string = "MyAccount.VNext.PasswordReset";
}

interface JWTTokenPayload extends JwtPayload {
  roles?: any;
}

export const parseRoles = (token: string): string[] => {
  let decodedToken = jwtDecode<JWTTokenPayload>(token);
  return decodedToken.roles || [];
};
