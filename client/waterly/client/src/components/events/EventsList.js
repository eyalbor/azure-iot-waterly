import React from 'react'
import { connect } from 'react-redux'
import { fetchEvents } from '../../actions'
import { renderTime } from '../../actions/timestamp'

class EventsList extends React.Component {

    componentDidMount(){
        if(this.props.currentUserId){
            this.props.fetchEvents(this.props.deviceId);
        }
    }

    renderList(){

        if(this.props.isSignedIn){
            return this.props.events.map(event => {
                    return(
                        <div className="item" key={event.id}>
                            <i className="large middle aligned icon bolt"/>
                            <div className="content">
                                <div className="water_read"><b>Meter reading:</b> {event.water_read}</div>
                                <div className="timestamp"><b>Reading time:</b> {renderTime(event.timestamp)}</div>
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