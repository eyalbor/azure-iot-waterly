import React from 'react';
import Footer from './Footer'
import './HomePage.css'

class HomePage extends React.Component {
    render() {
        return (
            <div>
                <div className="ui vertical masthead center aligned basic segment">
                    <div className="ui text container" style={{padding: "120px"}}>
                        <h2 className="ui header">
                            Your Water Consumption Just Got Smarter
                        </h2>
                        <img alt="logo" src="/logo300s.png" className="ui centered large image"/>
                    </div>
                </div>
                <Footer className="footer"/>
            </div>
        );
    }
  }

export default HomePage;
