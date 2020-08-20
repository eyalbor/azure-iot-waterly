import React from 'react'
import { Redirect } from 'react-router-dom'
import { connect } from 'react-redux'

class ProtectedRoute extends React.Component {

    render() {
        const Component = this.props.component;
        const isAuthenticated = this.props.isSignedIn
        console.log(isAuthenticated)
        return isAuthenticated ? (
            <Component />
        ) : (
            <Redirect to={{ pathname: '/' }} />
        );
    }
}

const mapStateToProps = (state) => {
    return {
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps,null)(ProtectedRoute);