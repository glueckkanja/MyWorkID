import { JwtPayload, jwtDecode } from "jwt-decode";

export abstract class Role {
  static ALLOW_CREATE_TAP = "MyWorkID.CreateTAP";
  static ALLOW_DISMISS_USER_RISK = "MyWorkID.DismissUserRisk";
  static ALLOW_CHANGE_PASSWORD = "MyWorkID.PasswordReset";
  static ALLOW_VALIDATE_IDENTITY = "MyWorkID.ValidateIdentity";
}

interface JWTTokenPayload extends JwtPayload {
  roles?: string[];
}

export const parseRoles = (token: string): string[] => {
  const decodedToken = jwtDecode<JWTTokenPayload>(token);
  return decodedToken.roles || [];
};
