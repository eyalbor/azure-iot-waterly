import React from 'react'
import { connect } from 'react-redux';
import { signIn, signOut } from '../actions'

class GoogleAuth extends React.Component {
  
    componentDidMount() {
        window.gapi.load('client:auth2', () => {
            //async callback
            window.gapi.client.init({
                clientId: '97255165694-plmujvb4e98sbnj76csld7s6l9jub3tr.apps.googleusercontent.com',
                scope: 'email'
            }).then(() => {
                //init return promise
                this.auth = window.gapi.auth2.getAuthInstance();
                //update component state
                this.onAuthChange(this.auth.isSignedIn.get());
                //so we need to init isSignedIn when component create
                this.auth.isSignedIn.listen(this.onAuthChange);
            });
        });
    }

    //this is call back func so we use arrow func
    onAuthChange = (isSignedIn) => {
        if(isSignedIn){
            this.props.signIn(this.auth.currentUser.get().getId());
        } else {
            this.props.signOut();
        }
    };

    onSignInClick = () => {
        this.auth.signIn();
    };

    onSignOutClick = () => {
        this.auth.signOut();
    };

    renderAuthButton() {
        if(this.props.isSignedIn === null ){
            return null;
        } else if (this.props.isSignedIn) {
            return (
                //we not write this.onSignOut() because we dont want to to call this method when component is rendered 
                <button onClick={this.onSignOutClick} className="ui red google button">
                    <i className="google icon" />
                    Sign Out
                </button>
            );
        } else {
            return (
                <button onClick={this.onSignInClick} className="ui red google button">
                    <i className="google icon" />
                    Sign In
                </button>
            );
        }
    }

    render(){
        return <div>{this.renderAuthButton()}</div>
    }
}

const mapStateToProps = (state) => {
    return {isSignedIn: state.auth.isSignedIn}
};

export default connect(mapStateToProps,
    {signIn, signOut}
)(GoogleAuth);