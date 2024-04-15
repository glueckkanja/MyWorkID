import { JwtPayload, jwtDecode } from "jwt-decode";

export abstract class Role {
  static ALLOW_CREATE_TAP = "MyAccount.VNext.CreateTAP";
  static ALLOW_DISMISS_USER_RISK = "MyAccount.VNext.DismissUserRisk";
  static ALLOW_CHANGE_PASSWORD = "MyAccount.VNext.PasswordReset";
  static ALLOW_VALIDATE_IDENTITY = "MyAccount.VNext.ValidateIdentity";
}

interface JWTTokenPayload extends JwtPayload {
  roles?: any;
}

export const parseRoles = (token: string): string[] => {
  const decodedToken = jwtDecode<JWTTokenPayload>(token);
  return decodedToken.roles || [];
};
