import React from 'react';
import { Link } from 'react-router-dom';
// import PageNotFound from '../assets/images/PageNotFound';
class NoFound extends React.Component{
    render(){
        return <div>
            {/* <img src={PageNotFound}  /> */}
            <h3>404 page not found</h3>
            <p>We are sorry but the page you are looking for does not exist.</p>
            <p style={{textAlign:"center"}}>
            <Link to="/">Go to Home </Link>
            </p>
    </div>
    }
}
export default NoFound;