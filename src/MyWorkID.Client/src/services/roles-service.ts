import { JwtPayload, jwtDecode } from "jwt-decode";

export abstract class Role {
  static readonly ALLOW_CREATE_TAP = "MyWorkID.CreateTAP";
  static readonly ALLOW_DISMISS_USER_RISK = "MyWorkID.DismissUserRisk";
  static readonly ALLOW_CHANGE_PASSWORD = "MyWorkID.PasswordReset";
  static readonly ALLOW_VALIDATE_IDENTITY = "MyWorkID.ValidateIdentity";
}

interface JWTTokenPayload extends JwtPayload {
  roles?: string[];
}

export const parseRoles = (token: string): string[] => {
  const decodedToken = jwtDecode<JWTTokenPayload>(token);
  return decodedToken.roles || [];
};
