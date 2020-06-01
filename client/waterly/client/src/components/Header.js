import React from 'react';
import { Link } from 'react-router-dom';
import GoogleAuth from './GoogleAuth'

const Header = () => {
    return (
        <div className="ui massive inverted menu">
            <Link to="/" className="header item">
                Waterly
            </Link>
            <div className="right menu">
                <Link to="/devices/list" className="item">
                    All Devices
                </Link>
                <Link to="/bill/show" className="item">
                    Pay Bill
                </Link>
                <div className="item">
                    <GoogleAuth/>
                </div>
            </div>
        </div>
        );
};

export default Header;