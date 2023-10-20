import { useEffect } from "react";
import { handleActionAuthRedirect } from "../services/MsalService";
import { dismissUserRisk } from "../services/ApiService";

const App = () => {
  useEffect(() => {
    handleActionAuthRedirect();
  }, []);

  return (
    <div className="App">
      <button>Password Reset</button>
      <button>Create Temporary Access Password</button>
      <button
        onClick={dismissUserRisk}
      >
        Dismiss User Risk
      </button>
    </div>
  );
};

export default App;
