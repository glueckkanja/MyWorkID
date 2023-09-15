import { useEffect } from "react";
import { authenticateRequest, getMsalInstance } from "../services/MsalService";
import { REQUEST_TYPE } from "../types";
import { Strings } from "../Strings";

const App = () => {

  useEffect(() => {
    getMsalInstance().then((msalInstance) => {
      msalInstance.handleRedirectPromise().then((res) => {
        if (res?.state) {
          switch (res.state) {
            case "dismissUserRisk":
              authenticateRequest(
                "https://localhost:7093/DismissUserRisk",
                REQUEST_TYPE.PUT,
                Strings.DISMISS_USER_RISK
              );
              break;
            default:
              return;
          }
        }
      });
    });
  }, []);

  return (
    <div className="App">
      <button>Password Reset</button>
      <button>Create Temporary Access Password</button>
      <button
        onClick={() => {
          authenticateRequest(
            "https://localhost:7093/DismissUserRisk",
            REQUEST_TYPE.PUT,
            Strings.DISMISS_USER_RISK
          );
        }}
      >
        Dismiss User Risk
      </button>
    </div>
  );
};

export default App;
