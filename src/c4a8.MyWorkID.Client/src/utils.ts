import { TWWWAuthenticateHeader } from "./types";

export const parseChallenges = (header: string): TWWWAuthenticateHeader => {
  const schemeSeparator = header.indexOf(" ");
  const challenges = header.substring(schemeSeparator + 1).split(",");
  const challengeMap: Record<string, string> = {};

  challenges.forEach((challenge) => {
    const [key, value] = challenge.split("=");
    challengeMap[key.trim()] = window.decodeURI(value.replace(/['"]+/g, ""));
  });

  return challengeMap as TWWWAuthenticateHeader;
};
