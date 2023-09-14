import React from 'react';
import { authenticateRequest } from '../services/MsalService'
import { REQUEST_TYPE } from "../types";

const App = () => {
  return (
    <div className="App">
      <button>Password Reset</button>
      <button>Create Temporary Access Password</button>
      <button onClick={(event) => {
        authenticateRequest("https://localhost:7093/DismissUserRisk", REQUEST_TYPE.PUT)
    }}>Dismiss User Risk</button>
    </div>
  );
}

export default App;
