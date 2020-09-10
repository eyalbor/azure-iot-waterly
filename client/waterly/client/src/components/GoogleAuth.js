import React from 'react'
import { connect } from 'react-redux';
import { signIn, signOut, setUser } from '../actions'
import { oAuth } from '../keys'

class GoogleAuth extends React.Component {
  
    componentDidMount() {
        window.gapi.load('client:auth2', () => {
            //async callback
            window.gapi.client.init({
                clientId: oAuth,
                scope: 'email'
            }).then(() => {
                //init return promise
                this.auth = window.gapi.auth2.getAuthInstance();
                //email
                //(this.auth.currentUser.le.Da)
                //update component state
                this.onAuthChange(this.auth.isSignedIn.get());
                //so we need to init isSignedIn when component create
                this.auth.isSignedIn.listen(this.onAuthChange);
                this.props.setUser(this.auth.currentUser)
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
        } else if (this.props.isSignedIn && this.props.user) {
            //console.log(this.props.user)
            return (
                <div>
                    {/* we not write this.onSignOut() because we dont want to to call this method when component is rendered  */}
                    <button onClick={this.onSignOutClick} className="ui red google button">
                        <i className="google icon" />
                        Sign Out
                    </button>
                    <br/>
                    <div style={{fontSize : '12px', padding:'5px'}}>Hello, {this.props.user.le.tt.Ad}</div>
                </div>
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
    return {
        isSignedIn: state.auth.isSignedIn,
        user: state.auth.user
    }
};

export default connect(mapStateToProps,
    {signIn, signOut, setUser}
)(GoogleAuth);

//https://stackoverflow.com/questions/43164554/how-to-implement-authenticated-routes-in-react-router-4/43171515#43171515
//https://stackoverflow.com/questions/47476186/when-user-is-not-logged-in-redirect-to-login-reactjs
//https://tylermcginnis.com/react-router-protected-routes-authentication/