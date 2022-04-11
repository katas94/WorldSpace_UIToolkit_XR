import { Renderer } from '@reactunity/renderer';
import * as React from 'react';
import './index.scss';

function App() {
  return <scroll>
    <div style={{backgroundColor: "blue"}}>hola</div> 
    <text>{`Go to <color=red>app.tsx</color> to edit this file`}</text>
    <button>Hola que tal</button>
    <div>esto es un div</div>
  </scroll>;
}

Renderer.render(<App />);
