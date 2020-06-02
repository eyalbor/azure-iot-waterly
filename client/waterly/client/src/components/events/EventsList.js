import React from 'react'
import { connect } from 'react-redux'
import { fetchEvents } from '../../actions'

class EventsList extends React.Component {

    componentDidMount(){
        if(this.props.currentUserId){
            this.props.fetchEvents(this.props.deviceId);
        }
    }

    renderTime(timestamp){
        // sometimes even the US needs 24-hour time
        let options = {
            year: 'numeric', month: 'numeric', day: 'numeric',
            hour: 'numeric', minute: 'numeric', second: 'numeric',
            hour12: false,
            timeZone: Intl.DateTimeFormat().resolvedOptions().timeZone 
        };
        // to specify options but use the browser's default locale, use 'default'
        return new Intl.DateTimeFormat('default', options).format(timestamp);
    }

    renderList(){

        if(this.props.isSignedIn){
            return this.props.events.map(event => {
                    return(
                        <div className="item" key={event.id}>
                            <i className="large middle aligned icon bolt"/>
                            <div className="content">
                                <div className="water_read">Water Read: {event.water_read}</div>
                                <div className="timestamp">Time: {this.renderTime(event.timestamp*1000)}</div>
                            </div>
                        </div>
                    );
                })
            
        } else {
            return <h3>Please sign in</h3>
        }
       
    }

    render(){
        return (
            <div className="ui container">
                <h2>Device <u>{this.props.deviceId}</u> Events</h2>
                <div className="ui celled list">{this.renderList()}</div>
            </div>
        );
    }
};

const mapStateToProps = (state, ownProps) => {
    return {
        deviceId: ownProps.match.params.id,
        events: Object.values(state.events),
        currentUserId: state.auth.userId,
        isSignedIn: state.auth.isSignedIn
    }
}

export default connect(mapStateToProps,{fetchEvents})(EventsList);